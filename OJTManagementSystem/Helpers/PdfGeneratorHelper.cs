using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OJTManagementSystem.Models;
using System.Text;

namespace OJTManagementSystem.Helpers
{
    public class PdfGeneratorHelper
    {
        public async Task<byte[]> GenerateCertificatePdfAsync(Intern intern, decimal totalHours)
        {
            return await Task.Run(() =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(memoryStream))
                    {
                        var pdf = new PdfDocument(writer);
                        var document = new iText.Layout.Document(pdf);

                        // ✅ FIXED: Use HELVETICA_BOLD font instead of SetBold()
                        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        var titleParagraph = new iText.Layout.Element.Paragraph("CERTIFICATE OF INTERNSHIP")
                            .SetFont(boldFont)
                            .SetFontSize(24)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(30);
                        document.Add(titleParagraph);

                        var dateParagraph = new iText.Layout.Element.Paragraph($"Date Issued: {DateTime.UtcNow:MMMM dd, yyyy}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(20);
                        document.Add(dateParagraph);

                        document.Add(new iText.Layout.Element.Paragraph("\n"));

                        var toWhomParagraph = new iText.Layout.Element.Paragraph("TO WHOM IT MAY CONCERN:")
                            .SetMarginBottom(15);
                        document.Add(toWhomParagraph);

                        var bodyParagraph = new iText.Layout.Element.Paragraph(
                            $"This is to certify that {intern.User.FullName}, Student ID: {intern.StudentId}, " +
                            $"from {intern.School} has successfully completed an internship program at our organization. " +
                            $"The internship was conducted from {intern.StartDate:MMMM dd, yyyy} to {intern.EndDate:MMMM dd, yyyy}. " +
                            $"During this period, the intern has rendered a total of {totalHours:F2} hours of work in the {intern.Department} department. " +
                            $"The intern has demonstrated commitment, professionalism, and dedication to the assigned tasks.")
                            .SetTextAlignment(TextAlignment.JUSTIFIED)
                            .SetMarginBottom(20);
                        document.Add(bodyParagraph);

                        document.Add(new iText.Layout.Element.Paragraph("\n\n"));

                        var detailsTable = new iText.Layout.Element.Table(new float[] { 250, 250 })
                            .SetWidth(UnitValue.CreatePercentValue(100))
                            .SetMarginBottom(20);

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Internship Details")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("")));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Student Name:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(intern.User.FullName)));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Student ID:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(intern.StudentId)));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("School:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(intern.School)));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Course:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(intern.Course)));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Department:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(intern.Department)));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Start Date:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(intern.StartDate.ToString("MMMM dd, yyyy"))));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("End Date:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(intern.EndDate.ToString("MMMM dd, yyyy"))));

                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Total Hours:")));
                        detailsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph($"{totalHours:F2} hours")));

                        document.Add(detailsTable);

                        document.Add(new iText.Layout.Element.Paragraph("\n\n"));

                        var signatureParagraph = new iText.Layout.Element.Paragraph("_________________________\nAuthorized Officer\nOJT Management System")
                            .SetTextAlignment(TextAlignment.CENTER);
                        document.Add(signatureParagraph);

                        document.Close();
                    }

                    return memoryStream.ToArray();
                }
            });
        }
    }
}