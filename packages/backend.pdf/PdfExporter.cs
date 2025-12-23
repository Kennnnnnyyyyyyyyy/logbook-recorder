using System.Text.Json;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;

namespace Backend.Pdf;

public interface IPdfExporter
{
    Task<string> ExportDraftAsync(string templatePath, string draftId, string userId, JsonDocument formData, string? drawingPath, string contentRootPath);
}

public class PdfExporter : IPdfExporter
{
    public async Task<string> ExportDraftAsync(string templatePath, string draftId, string userId, JsonDocument formData, string? drawingPath, string contentRootPath)
    {
        return await Task.Run(() => ExportDraftSync(templatePath, draftId, userId, formData, drawingPath, contentRootPath));
    }

    private string ExportDraftSync(string templatePath, string draftId, string userId, JsonDocument formData, string? drawingPath, string contentRootPath)
    {
        // Create output directory
        var exportsDir = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "storage", "exports", userId));
        Directory.CreateDirectory(exportsDir);
        var outputPath = Path.Combine(exportsDir, $"{draftId}.pdf");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template file not found: {templatePath}");

        // Simple approach: read template bytes, write to output with retry logic
        byte[] templateBytes;
        try
        {
            templateBytes = File.ReadAllBytes(templatePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to read template: {ex.Message}", ex);
        }

        // Atomically write the PDF bytes to the final output file using retry logic
        // for Windows file lock scenarios (antivirus, indexing, or lingering handles).
        RetryFileOperation(() =>
        {
            // Delete old version if exists
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            // Write PDF bytes
            File.WriteAllBytes(outputPath, templateBytes);
        });

        return outputPath;
    }

    /// <summary>
    /// Retries a file operation (Delete, Move) up to 5 times with 100ms delay
    /// if IOException or UnauthorizedAccessException occurs (Windows file lock scenarios).
    /// </summary>
    private void RetryFileOperation(Action operation)
    {
        const int maxRetries = 5;
        const int delayMs = 100;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                operation();
                return;
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                System.Threading.Thread.Sleep(delayMs);
            }
            catch (UnauthorizedAccessException) when (attempt < maxRetries - 1)
            {
                System.Threading.Thread.Sleep(delayMs);
            }
        }

        // Final attempt without catch; let exception propagate if all retries exhausted
        operation();
    }

    private void FillFormFields(IDictionary<string, PdfFormField> fields, JsonDocument formData)
    {
        var root = formData.RootElement;

        foreach (var kvp in fields)
        {
            var fieldName = kvp.Key;
            var field = kvp.Value;

            if (root.TryGetProperty(fieldName, out var value))
            {
                try
                {
                    string strValue = value.ValueKind switch
                    {
                        System.Text.Json.JsonValueKind.True => "Yes",
                        System.Text.Json.JsonValueKind.False => "Off",
                        System.Text.Json.JsonValueKind.String => value.GetString() ?? "",
                        System.Text.Json.JsonValueKind.Number => value.GetRawText(),
                        _ => value.GetRawText()
                    };

                    field.SetValue(strValue);
                }
                catch
                {
                    // Silently skip fields that can't be filled
                }
            }
        }
    }

    private void AddFormDataOverlay(Document document, JsonDocument formData)
    {
        try
        {
            // Create a small text box; layout engine will place it on page 1 by default
            var paragraph = new Paragraph()
                .SetFontSize(8)
                .SetMarginLeft(10)
                .SetMarginTop(10);

            paragraph.Add("Form Data:\n");

            var root = formData.RootElement;
            foreach (var property in root.EnumerateObject())
            {
                var val = property.Value.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => property.Value.GetString() ?? "(null)",
                    System.Text.Json.JsonValueKind.True => "true",
                    System.Text.Json.JsonValueKind.False => "false",
                    System.Text.Json.JsonValueKind.Number => property.Value.GetRawText(),
                    _ => property.Value.GetRawText()
                };

                paragraph.Add($"{property.Name}: {val}\n");
            }

            document.Add(paragraph);
        }
        catch
        {
            // Silently fail if overlay can't be added
        }
    }

    private void AddDrawingOverlay(Document document, PdfDocument pdfDoc, string drawingPath)
    {
        // Overlay drawing PNG on page 1 at a reasonable position
        try
        {
            var imageBytes = File.ReadAllBytes(drawingPath);
            var imageData = iText.IO.Image.ImageDataFactory.Create(imageBytes);
            var image = new Image(imageData);

            // Scale image reasonably (e.g., 150x150 px) and position at top-right
            image.SetWidth(150);
            image.SetHeight(150);
            var firstPage = pdfDoc.GetFirstPage();
            image.SetFixedPosition(1, firstPage.GetMediaBox().GetWidth() - 160, firstPage.GetMediaBox().GetHeight() - 160);
            document.Add(image);
        }
        catch
        {
            // Silently fail if drawing can't be added
        }
    }
}


