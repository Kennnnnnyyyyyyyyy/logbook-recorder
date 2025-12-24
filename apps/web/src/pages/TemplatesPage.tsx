import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getTemplates, Template } from '../api/templates';
import TemplateUploadForm from '../components/TemplateUploadForm';

export default function TemplatesPage() {
  const [templates, setTemplates] = useState<Template[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const fetchTemplates = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getTemplates();
      setTemplates(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch templates');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTemplates();
  }, []);

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Templates</h1>

      <TemplateUploadForm onUploaded={fetchTemplates} />

      {loading && <p>Loading templates...</p>}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && templates.length === 0 && <p>No templates found. Upload one to get started.</p>}

      {!loading && templates.length > 0 && (
        <div>
          <h2>Available Templates</h2>
          <ul>
            {templates.map((tpl) => (
              <li key={tpl.id} style={{ marginBottom: '1rem' }}>
                <strong>{tpl.title}</strong>
                {tpl.collegeName && <p>College: {tpl.collegeName}</p>}
                <p style={{ fontSize: '0.9em', color: '#666' }}>
                  Created: {new Date(tpl.createdAtUtc).toLocaleString()}
                </p>
                <button onClick={() => navigate(`/editor/${tpl.id}`)}>Edit</button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
