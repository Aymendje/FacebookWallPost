using System;
using System.Windows;
using Facebook;

// https://stackoverflow.com/questions/20353702/facebook-oauth-in-wpf-c-sharp-example
// https://stackoverflow.com/questions/25266197/oauthexception-200-200-the-user-hasnt-authorized-the-application-to-per
// http://www.markhagan.me/Samples/Grant-Access-And-Post-As-Facebook-User-ASPNet

namespace FacebookWallPost
{
    /// <summary>
    /// Interaction logic for FacebookAuthenticationWindow.xaml
    /// </summary>
    public partial class FacebookAuthenticationWindow : Window
    {
        //The Application ID from Facebook
        public string AppID { get; set; }

        //The access token retrieved from facebook's authentication
        public string AccessToken { get; set; }

        //The message we want to display
        public string MyMessage { get; set; }

        public FacebookAuthenticationWindow()
        {
            AppID = "__APP__ID__"; // TODO : Put your APP ID here (a long string of number)
            MyMessage = "Hello World !";   // We can put what ever we want, or even recive it from another app
            InitializeComponent();
            this.Loaded += (object sender, RoutedEventArgs e) =>
            {
                //Add the message hook in the code behind since I got a weird bug when trying to do it in the XAML
                webBrowser.MessageHook += webBrowser_MessageHook;

                //Delete the cookies since the last authentication
                DeleteFacebookCookie();

                //Create the destination URL
                var destinationURL = String.Format("https://www.facebook.com/dialog/oauth?client_id={0}&scope={1}&display=popup&redirect_uri=http://www.facebook.com/connect/login_success.html&response_type=token",
                   AppID, //client_id
                   "email,user_birthday,publish_actions" //scope
                );
                webBrowser.Navigate(destinationURL);
            };
        }


        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //If the URL has an access_token, grab it and walk away...
            var url = e.Uri.Fragment;
            if (url.Contains("access_token") && url.Contains("#"))
            {
                url = (new System.Text.RegularExpressions.Regex("#")).Replace(url, "?", 1);
                AccessToken = System.Web.HttpUtility.ParseQueryString(url).Get("access_token");
                this.Close();

                var client = new FacebookClient(AccessToken);
                client.Post("/me/feed", new { message = MyMessage });

            }


        }

        private void DeleteFacebookCookie()
        {
            //Set the current user cookie to have expired yesterday
            string cookie = String.Format("c_user=; expires={0:R}; path=/; domain=.facebook.com", DateTime.UtcNow.AddDays(-1).ToString("R"));
            System.Windows.Application.SetCookie(new Uri("https://www.facebook.com"), cookie);
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.LocalPath == "/r.php")
            {
                System.Windows.MessageBox.Show("To create a new account go to www.facebook.com", "Could Not Create Account", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }

        IntPtr webBrowser_MessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //msg = 130 is the last call for when the window gets closed on a window.close() in javascript
            if (msg == 130)
            {
                this.Close();
            }
            return IntPtr.Zero;
        }
    }
}
