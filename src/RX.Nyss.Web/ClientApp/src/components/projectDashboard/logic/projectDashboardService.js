import jsPDF from "jspdf";
import domtoimage from "dom-to-image";

export const generatePdfDocument = async (containerElement) => {
  const reportFileName = "Report";

  const pageWidth = 210; // mm
  const pageHeight = 295; // mm
  const margin = 10; // mm
  const spacing = 5; // mm

  const contentWidth = pageWidth - margin * 2;

  const elements = containerElement.querySelectorAll('[data-printable]');

  let pdf = new jsPDF('p', 'mm', 'a4');
  let currentPositionY = margin;

  for (const element of elements) {
    const imageData = await domtoimage.toPng(element)
    const canvasHeightInMm = element.scrollHeight * contentWidth / element.scrollWidth;

    if (currentPositionY + canvasHeightInMm > (pageHeight - margin)) {
      pdf.addPage();
      currentPositionY = margin;
    }

    pdf.addImage(imageData, 'PNG', margin, currentPositionY, contentWidth, canvasHeightInMm);

    currentPositionY += canvasHeightInMm + spacing;
  }

  pdf.save(`${reportFileName}.pdf`);
};
