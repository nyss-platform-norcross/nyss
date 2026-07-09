import jsPDF from "jspdf";
import domtoimage from "dom-to-image";

export const generatePdfDocument = async (
  title,
  containerElement,
  reportFileName,
) => {
  const pageWidth = 210; // mm
  const pageHeight = 295; // mm
  const margin = 10; // mm
  const spacing = 5; // mm

  const contentWidth = pageWidth - margin * 2;

  const elements = containerElement.querySelectorAll("[data-printable]");

  let pdf = new jsPDF("p", "mm", "a4");
  let currentPositionY = margin;

  pdf.setFontSize(10);
  pdf.text(title, 105, currentPositionY + 2, "center");

  currentPositionY += 10;

  for (const element of elements) {
    const imageData = await domtoimage.toJpeg(element, {
      bgcolor: "white",
    });
    const canvasHeightInMm =
      (element.scrollHeight * contentWidth) / element.scrollWidth;

    if (currentPositionY + canvasHeightInMm > pageHeight - margin) {
      pdf.addPage();
      currentPositionY = margin;
    }

    pdf.addImage(
      imageData,
      "JPG",
      margin,
      currentPositionY,
      contentWidth,
      canvasHeightInMm,
    );

    currentPositionY += canvasHeightInMm + spacing;
  }

  pdf.save(`${reportFileName}.pdf`);
};
