
using System;
using System.Collections.Generic;
using System.Drawing;
using Colorful;
using Console = Colorful.Console;
using Spectre.Console;
using System.Threading;
using System.Text;

namespace ELMA_API {
    
    class Logging
    {
        static public void StartApp() 
        {
            List<String> descriptionApp = new List<String>();
            descriptionApp.Add(" The main purpose of the app is synchronized the data of practices from database university and elma server. The data");
            descriptionApp.Add(" which are in database but aren't in elma server will be uploaded automatically. Process of uploading entities is running");
            descriptionApp.Add(" from zero dependency entities to entities that have dependencies previous uploaded entities. The main pattern that the author ");
            descriptionApp.Add(" tried to adhere to was dependency injection but DI container wasn't used. Since the application does not imply big architecture");
            descriptionApp.Add(" software so the app doesn't use module structure of directories. The recieving the data from database \"Деканат\"");
            descriptionApp.Add(" and following views are being used \"ШвебельГруппы\", \"ШвебельПрактики\", \"ШвебельПрактикиБезКомпет\", \"ШвебельСтуденты\"\n");

            Colorful.FigletFont font = Colorful.FigletFont.Load("./FigletFont/3d.flf");
            Figlet figlet = new Figlet(font);
            
            Console.Write("\n");
            Console.WriteLine(figlet.ToAscii(" ELMA-API"), System.Drawing.Color.FromArgb(36,114,200,255));

            int r = 225; int g = 255; int b = 250;

            for (int i = 0; i < descriptionApp.Count; i++)
            {
                Console.WriteLine(descriptionApp[i], System.Drawing.Color.FromArgb(r, g, b));
                r -= 18;
                b -= 9;
            }
        }

        static public void Info(String title, String message)
        {
            // separator title and message
            String sepTitleMessage = " >";
            if (title.ToLower() == InfoTitle.startUpload.ToLower()) sepTitleMessage = " /";

            var date = DateTime.Now;

            String resultStr = "";

            // time -> (<TIME>)
            resultStr += $"([dodgerblue2]{date.ToString("HH:mm:ss")}[/])";
            
            // -> (<TIME>) info
            resultStr += " [green3 invert bold]info[/]";
            
            // TITLE
            // ! logic color Theme for title
            resultStr += $@" {(title.ToUpper() == InfoTitle.startUpload.ToUpper()
                    ? "[orangered1]" 
                    : "[green1]")}{title.ToLower()}[/]";
            
            // ! logic color Theme for separator between Title and Message
            resultStr += title.ToUpper() == InfoTitle.startUpload.ToUpper() 
                ? $"[orangered1]{sepTitleMessage}[/]" 
                : $"[white]{sepTitleMessage}[/]";
            
            // MESSAGE
            // ! logic color Theme for message
            resultStr += title.ToUpper() == InfoTitle.startUpload.ToUpper() 
                ? $"[orangered1] {message.ToLower()}[/]" 
                : $"[white] {message.ToLower()}[/]";

            // logging
            AnsiConsole.MarkupLine(resultStr);
        }
    
        static public void Warn(String title, String message) 
        {
            var date = DateTime.Now;

            String resultStr = "";

            // time -> (<TIME>)
            resultStr += '(' + $"[dodgerblue2]{date.ToString("HH:mm:ss")}[/]" + ')';
            // Console.Write($"[{date.ToString()}]", System.Drawing.Color.FromArgb(22, 138, 173));

            // -> (...) <WARN>
            resultStr += " [red3 invert bold]warn[/]";

            // * Title
            resultStr += $" [red1]{title.ToLower()}[/]";

            resultStr += $" [white]>[/]";

            // * Message
            resultStr += $" [white]{message}[/]";

            // logging
            AnsiConsole.MarkupLine(resultStr);
        }
    
        static public void Testing()
        {
            // System.Console.OutputEncoding = Encoding.UTF8;
            // Synchronous
            AnsiConsole.Status()
                .Spinner(Spinner.Known.BouncingBar)
                .SpinnerStyle(Style.Parse("green1"))
                .Start("[dodgerblue1]Thinking[/]", ctx => 
                {
                    // Simulate some work
                    AnsiConsole.MarkupLine("[orange1]Doing some work...[/]");
                    Thread.Sleep(1000);
                    
                    // Update the status and spinner
                    ctx.Status("[dodgerblue1]Thinking some more[/]");

                    // ctx.SpinnerStyle(Style.Parse("green"));

                    // Simulate some work
                    AnsiConsole.MarkupLine("Doing some more work...");
                    Thread.Sleep(3000);
                });

        }
    }


    public static class InfoTitle 
    {
        // учебные планы
        public static readonly string launch = "launch";
        public static readonly string loginElma = "login-elma";
        public static readonly string loginDB = "login-db";
        public static readonly string startUpload = "start-upload";
        public static readonly string dataElma = "data-elma";
        public static readonly string dataDB = "data-db";
        public static readonly string missed = "missed";
        public static readonly string injectData = "inject-data";
        public static readonly string fileExists = "file-exists";
    }

    public static class WarnTitle 
    {
        public static readonly string fileNotFount = "not-found";
        public static readonly string directory = "directory";
        public static readonly string notFoundFaculty = "not-found-faculty";
        public static readonly string notFoundDirectPrep = "not-found-direct-prep";
    }

}