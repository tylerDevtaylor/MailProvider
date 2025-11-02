using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public class LocalServerCodeReceiverAutoClose : ICodeReceiver
{
    private const string SuccessResponse = @"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Authentication Successful</title>
            <style>
                body { font-family: Arial, sans-serif; text-align: center; padding: 50px; }
                .success { color: #4CAF50; font-size: 24px; margin-bottom: 20px; }
            </style>
        </head>
        <body>
            <div class='success'> Authentication Successful!</div>
            <p>You can close this window now.</p>
            <script>
                window.setTimeout(function() {
                    window.close();
                }, 3000);
            </script>
        </body>
        </html>";

    private const string ErrorResponse = @"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Authentication Failed</title>
            <style>
                body { font-family: Arial, sans-serif; text-align: center; padding: 50px; }
                .error { color: #f44336; font-size: 24px; margin-bottom: 20px; }
            </style>
        </head>
        <body>
            <div class='error'>✗ Authentication Failed</div>
            <p>Please close this window and try again.</p>
            <script>
                window.setTimeout(function() {
                    window.close();
                }, 3000);
            </script>
        </body>
        </html>";

    private readonly string _redirectUri;
    private readonly int _port;

    public LocalServerCodeReceiverAutoClose(int port = 8080)
    {
        _port = port;
        _redirectUri = $"http://127.0.0.1:{_port}/authorize/";
    }

    public string RedirectUri => _redirectUri;

    public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(
        AuthorizationCodeRequestUrl url,
        CancellationToken taskCancellationToken)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(_redirectUri);

        listener.Start();

        try
        {
            // Open browser
            Process.Start(new ProcessStartInfo(url.Build().ToString()) { UseShellExecute = true });

            // Wait for the authorization response
            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            // Parse the query string
            var queryString = request.Url.Query;
            var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);

            // Send response to browser
            byte[] buffer;
            if (queryParams["error"] != null)
            {
                buffer = System.Text.Encoding.UTF8.GetBytes(ErrorResponse);
            }
            else
            {
                buffer = System.Text.Encoding.UTF8.GetBytes(SuccessResponse);
            }

            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";

            // Write the response
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

            // Close the output stream BEFORE stopping the listener
            response.OutputStream.Close();
            response.Close();

            // Return the authorization code response
            return new AuthorizationCodeResponseUrl
            {
                Code = queryParams["code"],
                State = queryParams["state"],
                Error = queryParams["error"],
                ErrorDescription = queryParams["error_description"],
                ErrorUri = queryParams["error_uri"]
            };
        }
        finally
        {
            // Stop and close the listener after the response is sent
            listener.Stop();
            listener.Close();
        }
    }
}