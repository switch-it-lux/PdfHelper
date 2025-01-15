# Sit.PdfHelper
Sit.PdfHelper is a .Net standard 2.0 library that allows to execute some simple basic PDF manipulations such as:
- Spliting/merging PDF document.
- Adding basic elements (text, barcode, image) to PDF document.
- Searching for text/barcode locations.
- Converting image/email to a PDF document.
- Appling HOCR texts to an OCRed PDF document to make it searchable .
- Generating and assigning thumbnail to PDF document.

## Installation
Either checkout this Github repository or install via NuGet Package Manager.
If you want to use NuGet just search for "Sit.PdfHelper" or run the following command in the NuGet Package Manager console:
```bash
PM> Install-Package Sit.PdfHelper
```
## Usage
Instanciate the Sit.Pdf.PdfHelper class and call the required methods. 

#### Sample 1: Split PDF document by max size
```csharp
using var pdfHelper = new Sit.Pdf.PdfHelper("C:\\Temp\\sample.pdf");
var docs = pdfHelper.SplitByMaxSize(100000); //A file should not exceed 100Kb
for (int i = 0; i < docs.Count; i++) {
    File.WriteAllBytes($"C:\\Temp\\sample ({i + 1}).pdf", docs[i]);
}
```

#### Sample 2: Convert email to PDF document
```csharp
using var pdfHelper = await Sit.Pdf.PdfHelper.FromEmailAsync("C:\\Temp\\sample.eml", EmailType.Mime, PageSizes.A4, includeAttachments: true);
pdfHelper.Save($"C:\\Temp\\sample.pdf");
```

#### Sample 3: Generate thumbnail image for first page and store it to the PDF document
```csharp
using var pdfHelper = new PdfHelper("C:\\Temp\\sample.pdf");
var img = pdfHelper.GenerateThumbnail(page: 1, width: 200, whiteBackground: true);
pdfHelper.SetThumbnail(img, 1);
pdfHelper.Save("C:\\Temp\\sample with thumbnail.pdf");
```

#### Sample 4: Add and then find QR code
```csharp
// Adding a QR code to a new PDF
using var pdfHelper = new Sit.Pdf.PdfHelper(PageSizes.A4);
pdfHelper.AddElements(new PdfQrcodeElement {
    Page = 1,
    X = 10,
    Y = 110,
    Anchor = PdfElementAnchor.BaselineLeft,
    Color = System.Drawing.Color.Black,
    Size = 100,
    Text = "http://www.switch.lu"
});
pdfHelper.Save($"C:\\Temp\\qr.pdf");

// Finding QR code
var results = pdfHelper.GetBarcodeLocations(stopAfterXMatch: 1);
if (results.Length > 0) {
    Debug.WriteLine("Text: " + results[0].Text);
    Debug.WriteLine("Page: " + results[0].Location.Page);
    Debug.WriteLine("Location: " + results[0].Location.XLeft + "," + results[0].Location.YBottom);
}
```

Find more usage examples into '\src\Tests\PdfHelperTests\'.

## Legal information and credits
Sit.PdfHelper is a project by [Switch IT](https://www.switch.lu) licensed under the [GNU Affero General Public License](https://www.gnu.org/licenses/).

Third-party components:
- .Net Foundation components by Microsoft ([https://github.com/microsoft](https://github.com/microsoft), [https://github.com/dotnet](https://github.com/dotnet)).
  Licensed under the MIT License.
- HtmlAgilityPack by ZZZ Projects, Simon Mourrier, Jeff Klawiter and Stephan Grell ([https://github.com/zzzprojects/html-agility-pack](https://github.com/zzzprojects/html-agility-pack)).
  Licensed under the MIT License.
- iText7 by Apryse Software ([https://github.com/itext/itext-dotnet](https://github.com/itext/itext-dotnet)).
  Licensed under the GNU Affero General Public License.
- MailKit and MimeKit by Jeffrey Stedfast ([https://github.com/jstedfast/MailKit](https://github.com/jstedfast/MailKit)).
  Licensed under the MIT License.
- MsgReader by Kees van Spelde ([https://github.com/Sicos1977/MSGReader](https://github.com/Sicos1977/MSGReader)).
  Licensed under the MIT License.
- PDFtoImage by David Sungaila ([https://github.com/sungaila/PDFtoImage](https://github.com/sungaila/PDFtoImage)).
  Licensed under the MIT License.
- Newtonsoft.Json by James Newton-King ([https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)).
  Licensed under the MIT License.
- ZXing.Net by Michael Jahn ([https://github.com/micjahn/ZXing.Net](https://github.com/micjahn/ZXing.Net)).
  Licensed under the Apache 2.0 license.
