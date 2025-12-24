import { useState } from 'react';
import { uploadTemplate } from '../api/templates';

interface TemplateUploadFormProps {
  onUploaded: () => void;
}

export default function TemplateUploadForm({ onUploaded }: TemplateUploadFormProps) {
  const [file, setFile] = useState<File | null>(null);
  const [title, setTitle] = useState('');
  const [collegeName, setCollegeName] = useState('');
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file || !title) {
      setError('File and title are required');
      return;
    }

    setUploading(true);
    setError(null);
    try {
      await uploadTemplate(file, title, collegeName);
      setFile(null);
      setTitle('');
      setCollegeName('');
      onUploaded();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed');
    } finally {
      setUploading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{ marginBottom: '2rem', padding: '1rem', border: '1px solid #ccc' }}>
      <h3>Upload Template</h3>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      
      <div style={{ marginBottom: '1rem' }}>
        <label>
          Title: <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g., Lab Report"
            disabled={uploading}
          />
        </label>
      </div>

      <div style={{ marginBottom: '1rem' }}>
        <label>
          College Name: <input
            type="text"
            value={collegeName}
            onChange={(e) => setCollegeName(e.target.value)}
            placeholder="e.g., Engineering"
            disabled={uploading}
          />
        </label>
      </div>

      <div style={{ marginBottom: '1rem' }}>
        <label>
          PDF File: <input
            type="file"
            accept=".pdf,application/pdf"
            onChange={(e) => setFile(e.target.files?.[0] || null)}
            disabled={uploading}
          />
        </label>
      </div>

      <button type="submit" disabled={uploading}>
        {uploading ? 'Uploading...' : 'Upload Template'}
      </button>
    </form>
  );
}
