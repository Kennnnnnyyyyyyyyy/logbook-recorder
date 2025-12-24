import { useParams, Link } from "react-router-dom";
import { getTemplateFileUrl } from "../api/templates";

export default function EditorPage() {
  const { templateId } = useParams<{ templateId: string }>();

  if (!templateId) {
    return (
      <div style={{ padding: "2rem" }}>
        <h1>Editor</h1>
        <p>Missing templateId in URL.</p>
        <Link to="/templates">Back</Link>
      </div>
    );
  }

  const pdfUrl = getTemplateFileUrl(templateId);

  return (
    <div style={{ padding: "2rem" }}>
      <h1>Editor</h1>

      <div style={{ marginBottom: "1rem" }}>
        <Link to="/templates">← Back</Link>
        {"  |  "}
        <a href={pdfUrl} target="_blank" rel="noreferrer">
          Open PDF
        </a>
      </div>

      <div style={{ border: "1px solid #ccc", height: "80vh" }}>
        <iframe
          title="Template PDF"
          src={pdfUrl}
          style={{ width: "100%", height: "100%", border: "none" }}
        />
      </div>
    </div>
  );
}
