using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FinabitEmployee.Data;
using PuppeteerSharp;

public class SalarySlipGenerator
{
    public async Task<byte[]> GeneratePdfAsync(SalarySlip slip)
    {
        // Format numbers in INR (Indian Rupees)
        var cultureInfo = new CultureInfo("en-IN");

        string htmlContent = $@"
            <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Roboto', sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f5f7fa;
                            color: #333;
                        }}
                        .container {{
                            width: 100%;
                            max-width: 900px;
                            margin: 20px auto;
                            padding: 20px;
                            border: 1px solid #ddd;
                            border-radius: 10px;
                            background: #fff;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            text-align: center;
                            margin-bottom: 30px;
                            border-bottom: 2px solid #003366;
                            padding-bottom: 10px;
                        }}
                        .header img {{
                            width: 150px;
                            height: auto;
                            margin-bottom: 10px;
                        }}
                        .header h2 {{
                            color: #003366;
                            margin: 0;
                        }}
                        .details {{
                            margin-bottom: 30px;
                        }}
                        .details p {{
                            font-size: 14px;
                            margin: 5px 0;
                        }}
                        .salary-details {{
                            margin-top: 20px;
                        }}
                        .salary-details table {{
                            width: 100%;
                            border-collapse: collapse;
                            margin-top: 20px;
                        }}
                        .salary-details table th {{
                            background-color: #003366;
                            color: #fff;
                            padding: 10px;
                            text-align: left;
                        }}
                        .salary-details table td {{
                            padding: 10px;
                            border-bottom: 1px solid #ddd;
                        }}
                        .salary-details table tr:last-child td {{
                            font-weight: bold;
                            background-color: #f9f9f9;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 30px;
                            font-size: 12px;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='https://raw.githubusercontent.com/Rudratej2081/SigmaImages/refs/heads/master/finabits.png' alt='Company Logo' />
                            <h2>Salary Slip</h2>
                            <p>For {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(slip.Month)}, {slip.Year}</p>
                        </div>
                        <div class='details'>
                            <p><strong>Employee ID:</strong> {slip.EmployeeId}</p>
                            <p><strong>Name:</strong> {slip.FirstName} {slip.LastName}</p>
                            <p><strong>Email:</strong> {slip.Email}</p>
                            <p><strong>Phone Number:</strong> {slip.Phonenumber}</p>
                        </div>
                        <div class='salary-details'>
                            <h3>Salary Breakdown (INR)</h3>
                            <table>
                                <tr><th>Description</th><th>Amount</th></tr>
                                <tr><td>Basic Pay</td><td>₹{slip.BasicPay.ToString("N", cultureInfo)}</td></tr>
                                <tr><td>HRA</td><td>₹{slip.HRA.ToString("N", cultureInfo)}</td></tr>
                                <tr><td>Deductions</td><td>₹{slip.Deductions.ToString("N", cultureInfo)}</td></tr>
                                <tr><td><strong>Net Pay</strong></td><td><strong>₹{slip.NetPay.ToString("N", cultureInfo)}</strong></td></tr>
                            </table>
                        </div>
                        <div class='footer'>
                            <p>This document is system-generated and does not require a signature.</p>
                        </div>
                    </div>
                </body>
            </html>";

        // Initialize and download Chromium
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        using var page = await browser.NewPageAsync();

        // Set HTML content and generate PDF
        await page.SetContentAsync(htmlContent);

        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            PrintBackground = true
        });

        return pdfBytes;
    }
}
