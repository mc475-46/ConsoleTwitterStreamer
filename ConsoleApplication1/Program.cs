using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CoreTweet;
using CoreTweet.Streaming;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Tokens tokens = null;
            while(tokens == null)
            {
                try
                {
                    var session = OAuth.Authorize("eJWUnMOcfA4zZ1AYTKCmueS94",
                    "YLZa7OwqdVnk5w7vnEdoCCWCoGSrbHm1YvCB8SQWs0OOevoKZb");
                    Console.WriteLine($"Jump here: {session.AuthorizeUri}");
                    Console.Write("Input PIN here: ");
                    var PIN = Console.ReadLine();
                    tokens = OAuth.GetTokens(session, PIN);
                }
                catch (System.Net.WebException we)
                {
                    Console.WriteLine("No connection to the Internet. Press Enter key to close.");
                    Console.Read();
                    Environment.Exit(1);
                }
                catch (TwitterException te)
                {
                    Console.WriteLine("Invaild PIN Code.");
                }
            }
            Console.WriteLine("Succeeded connecting to Twitter!");

            foreach(var pastStatus in tokens.Statuses.HomeTimeline(10))
            {
                Console.WriteLine(FormatStatus(pastStatus));
            }

            var s =
                tokens.Streaming.UserAsObservable()
                .Where((StreamingMessage m) => m.Type == MessageType.Create)
                .Cast<StatusMessage>()
                .Select((StatusMessage m) => m.Status);

            s.Catch(
                s.DelaySubscription(TimeSpan.FromSeconds(3))
                    .Retry())
                    .Repeat();

            var stream = s.Subscribe(
                    status => Console.WriteLine(FormatStatus(status)),
                    (Exception ex) => Console.WriteLine(ex),
                    () => Console.WriteLine("終点")
                );

            while (true)
            {
                Thread.Sleep(100000000);
            }
            stream.Dispose();
        }

        static string FormatStatus(Status s)
        {
            var formatedString = "";
            formatedString += $"{s.User.Name} @{s.User.ScreenName}\n";
            formatedString += ((s.IsTruncated == true) ? s.FullText : s.Text) + "\n";
            formatedString += new String('-', 100);
            return formatedString;
        }
    }
}
