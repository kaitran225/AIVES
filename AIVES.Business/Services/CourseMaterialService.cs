using AIVES.Business.DTOs;
using AIVES.Business.Interfaces;
using AIVES.Data;
using AIVES.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace AIVES.Business.Services
{
    public class CourseMaterialService : ICourseMaterialService
    {
        private readonly AIVESDbContext _context;
        private const int MaxChunkSize = 500;
        private const int ChunkOverlap = 50;

        public CourseMaterialService(AIVESDbContext context) { _context = context; }

        public async Task<IEnumerable<CourseMaterialDto>> GetAllMaterialsAsync()
        {
            var materials = await _context.CourseMaterials.Include(m => m.Course).OrderByDescending(m => m.ImportedAt).ToListAsync();
            return materials.Select(MapToDto);
        }

        public async Task<IEnumerable<CourseMaterialDto>> GetMaterialsByCourseAsync(int courseId)
        {
            var materials = await _context.CourseMaterials.Include(m => m.Course).Where(m => m.CourseId == courseId).OrderByDescending(m => m.ImportedAt).ToListAsync();
            return materials.Select(MapToDto);
        }

        public async Task<CourseMaterialDto?> GetMaterialByIdAsync(int id)
        {
            var material = await _context.CourseMaterials.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == id);
            return material == null ? null : MapToDto(material);
        }

        public async Task<MaterialImportResultDto> ImportMaterialAsync(CourseMaterialCreateDto dto)
        {
            var material = new CourseMaterial { Title = dto.Title, Content = dto.Content, CourseId = dto.CourseId, SourceType = dto.SourceType, FileName = dto.FileName, ImportedAt = DateTime.Now };
            if (dto.FileUpload != null && dto.FileUpload.Length > 0)
            {
                using var reader = new StreamReader(dto.FileUpload.OpenReadStream());
                material.Content = await reader.ReadToEndAsync();
                material.FileName = dto.FileUpload.FileName;
                material.MimeType = dto.FileUpload.ContentType;
            }
            var chunks = ChunkContent(material.Content);
            material.ChunkCount = chunks.Count;
            material.Chunks = chunks;
            _context.CourseMaterials.Add(material);
            await _context.SaveChangesAsync();
            return new MaterialImportResultDto { Success = true, Message = $"Material imported. Split into {chunks.Count} chunks.", MaterialId = material.Id, ChunkCount = chunks.Count };
        }

        public async Task<CourseMaterialDto> UpdateMaterialAsync(int id, string title, string content)
        {
            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null) throw new KeyNotFoundException($"Material {id} not found.");
            material.Title = title; material.Content = content;
            var chunks = ChunkContent(content);
            _context.MaterialChunks.RemoveRange(material.Chunks);
            material.Chunks = chunks; material.ChunkCount = chunks.Count;
            await _context.SaveChangesAsync();
            return MapToDto(material);
        }

        public async Task DeleteMaterialAsync(int id)
        {
            var material = await _context.CourseMaterials.Include(m => m.Chunks).FirstOrDefaultAsync(m => m.Id == id);
            if (material != null) { _context.MaterialChunks.RemoveRange(material.Chunks); _context.CourseMaterials.Remove(material); await _context.SaveChangesAsync(); }
        }

        public async Task<string> RetrieveRelevantChunksAsync(string courseCode, string query, int maxChunks = 5)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Code.Equals(courseCode, StringComparison.OrdinalIgnoreCase));
            if (course == null) return $"[No materials found] No course found with code: {courseCode}";
            var chunks = await _context.MaterialChunks.Where(mc => mc.Material.CourseId == course.Id).ToListAsync();
            if (!chunks.Any()) return $"[No materials found] No materials for course: {courseCode}";
            var queryTerms = ExtractKeywords(query);
            var scoredChunks = chunks.Select(chunk => new { Chunk = chunk, Score = CalculateSimilarity(chunk.Text, queryTerms) }).OrderByDescending(s => s.Score).Take(maxChunks).ToList();
            if (!scoredChunks.Any(s => s.Score > 0)) return $"[No relevant materials] No materials matching: {query}";
            var result = new StringBuilder();
            foreach (var item in scoredChunks.Where(s => s.Score > 0))
            {
                result.AppendLine($"--- Chunk {item.Chunk.ChunkIndex} (relevance: {item.Score:F2}) ---");
                result.AppendLine(item.Chunk.Text); result.AppendLine();
            }
            return result.ToString();
        }

        private List<MaterialChunk> ChunkContent(string content)
        {
            var chunks = new List<MaterialChunk>();
            if (string.IsNullOrWhiteSpace(content)) return chunks;
            var paragraphs = Regex.Split(content, "(?<=[.!?])\\s+");
            var currentChunk = new StringBuilder(); var chunkIndex = 0;
            foreach (var paragraph in paragraphs)
            {
                if (currentChunk.Length == 0) { currentChunk.Append(paragraph); }
                else if (currentChunk.Length + paragraph.Length <= MaxChunkSize) { currentChunk.Append(" "); currentChunk.Append(paragraph); }
                else
                {
                    chunks.Add(new MaterialChunk { Text = currentChunk.ToString().Trim(), ChunkIndex = chunkIndex });
                    chunkIndex++;
                    var overlapText = currentChunk.ToString().Substring(Math.Max(0, currentChunk.Length - ChunkOverlap));
                    currentChunk.Clear(); currentChunk.Append(overlapText); currentChunk.Append(" "); currentChunk.Append(paragraph);
                }
            }
            if (currentChunk.Length > 0) chunks.Add(new MaterialChunk { Text = currentChunk.ToString().Trim(), ChunkIndex = chunkIndex });
            return chunks;
        }

        private HashSet<string> ExtractKeywords(string text)
        {
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "the", "a", "an", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "may", "might", "shall", "can", "to", "of", "in", "for", "on", "with", "at", "by", "from", "as", "into", "through", "during", "and", "but", "or", "not", "this", "that", "these", "those", "it", "its", "i", "you", "he", "she", "we", "they", "what", "which", "who", "how", "when", "where", "why" };
            return Regex.Split(text.ToLower(), "[^a-z0-9]+").Where(w => w.Length > 2 && !stopWords.Contains(w)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private int CalculateSimilarity(string text, HashSet<string> queryTerms)
        {
            if (string.IsNullOrWhiteSpace(text) || !queryTerms.Any()) return 0;
            var textLower = text.ToLower(); int score = 0;
            foreach (var term in queryTerms) { int count = 0, index = 0; while ((index = textLower.IndexOf(term, index, StringComparison.Ordinal)) != -1) { count++; index += term.Length; } score += count; }
            return score;
        }

        private CourseMaterialDto MapToDto(CourseMaterial material)
        {
            return new CourseMaterialDto { Id = material.Id, Title = material.Title, Content = material.Content, CourseId = material.CourseId, CourseName = material.Course?.Name, SourceType = material.SourceType, FileName = material.FileName, ChunkCount = material.ChunkCount, ImportedAt = material.ImportedAt };
        }
    }
}
