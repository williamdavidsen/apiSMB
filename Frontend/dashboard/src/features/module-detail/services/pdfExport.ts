export async function exportElementAsPdf(element: HTMLElement, filename: string) {
  const [{ default: html2canvas }, { jsPDF }] = await Promise.all([import('html2canvas'), import('jspdf')])
  const canvas = await html2canvas(element, {
    scale: 2,
    useCORS: true,
    backgroundColor: '#ffffff',
  })

  const imageData = canvas.toDataURL('image/png')
  const pdf = new jsPDF('p', 'mm', 'a4')
  const pageWidth = pdf.internal.pageSize.getWidth()
  const pageHeight = pdf.internal.pageSize.getHeight()
  const margin = 10
  const contentWidth = pageWidth - margin * 2
  const imageHeight = (canvas.height * contentWidth) / canvas.width

  let remainingHeight = imageHeight
  let yOffset = 0
  pdf.addImage(imageData, 'PNG', margin, margin, contentWidth, imageHeight)
  remainingHeight -= pageHeight - margin * 2

  while (remainingHeight > 0) {
    yOffset += pageHeight - margin * 2
    pdf.addPage()
    pdf.addImage(imageData, 'PNG', margin, margin - yOffset, contentWidth, imageHeight)
    remainingHeight -= pageHeight - margin * 2
  }

  pdf.save(filename)
}
