using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    /// <summary>
    /// Imported learning materials (lectures, textbooks, documents) used for RAG-based question generation.
    /// </summary>
    public class CourseMaterial
    {
        public int Id { get; set; }

        /// <summary>
        /// Title of the material (e.g., "Chapter 3 - Data Structures")
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The full text content of the material (chunked for RAG)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Course this material belongs to
        /// </summary>
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        /// <summary>
        /// Source type: FileUpload, TextPaste, URL
        /// </summary>
        public string SourceType { get; set; } = "TextPaste";

        /// <summary>
        /// Original file name if uploaded from file
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// MIME type of the original file
        /// </summary>
        public string? MimeType { get; set; }

        /// <summary>
        /// Raw bytes of the uploaded file (if any)
        /// </summary>
        public byte[]? FileData { get; set; }

        /// <summary>
        /// Number of chunks this material was split into for RAG
        /// </summary>
        public int ChunkCount { get; set; }

        /// <summary>
        /// When the material was imported
        /// </summary>
        public DateTime ImportedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Active chunks for RAG retrieval
        /// </summary>
        public List<MaterialChunk> Chunks { get; set; } = new();
    }

    /// <summary>
    /// A chunk of text from a CourseMaterial, used for vector-like similarity matching in RAG.
    /// </summary>
    public class MaterialChunk
    {
        public int Id { get; set; }

        /// <summary>
        /// The chunk text (up to max length)
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Chunk index within the parent material (0-based)
        /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>
        /// The material this chunk belongs to
        /// </summary>
        public int MaterialId { get; set; }
        public CourseMaterial? Material { get; set; }
    }
}
