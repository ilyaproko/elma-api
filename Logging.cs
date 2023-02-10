
using System;
using System.Collections.Generic;
using System.Drawing;
using Colorful;
using Console = Colorful.Console;

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

            FigletFont font = FigletFont.Load("./FigletFont/3d.flf");
            Figlet figlet = new Figlet(font);
            
            Console.Write("\n");
            Console.WriteLine(figlet.ToAscii(" ELMA-API"), ColorTranslator.FromHtml("#8AFFEF"));

            int r = 225; int g = 255; int b = 250;

            for (int i = 0; i < descriptionApp.Count; i++)
            {
                Console.WriteLine(descriptionApp[i], Color.FromArgb(r, g, b));
                r -= 18;
                b -= 9;
            }
        }

        static public void Info(String title, String message)
        {

            // separator title and message
            String sepTitleMessage = " >";
            if (title.ToLower() == InfoTitle.start_upload.ToLower()) sepTitleMessage = " /";

            // transform message to UpperCase if Start-Upload title
            message = title.ToUpper() == InfoTitle.start_upload.ToUpper()
                ? message.ToUpper()
                : message;

            var date = DateTime.Now;
            
            Console.Write($"[{date.ToString()}]", Color.FromArgb(22, 138, 173));
            
            Console.Write(" info", Color.FromArgb(124, 181, 24));
            
            Console.Write(" /", Color.White);
            
            Console.Write($" {title.ToUpper()}", 
                title.ToUpper() == InfoTitle.start_upload.ToUpper()
                    ? Color.FromArgb(251, 97, 7) 
                    : Color.FromArgb(112, 224, 0)); // custom title of the INFO
            
            Console.Write(sepTitleMessage, title.ToUpper() == InfoTitle.start_upload.ToUpper()
                    ? Color.FromArgb(251, 97, 7) 
                    : Color.White);
            
            Console.Write($" {message}", 
                title.ToUpper() == InfoTitle.start_upload.ToUpper() 
                    ? Color.FromArgb(251, 97, 7) 
                    : Color.White); // custom message of the INFO
            
            Console.Write("\n"); // new line
        }
    
        static public void Warn(String title, String message) 
        {
            var date = DateTime.Now;
            Console.Write($"[{date.ToString()}]", Color.FromArgb(22, 138, 173));

            Console.Write(" warn", Color.FromArgb(178, 6, 0));

            Console.Write(" /", Color.White);

            Console.Write($" {title.ToUpper()}", Color.FromArgb(207, 10, 10));

            Console.Write(" >", Color.White);

            Console.Write($" {message}", Color.White);

            Console.Write("\n"); // new line
        }
    }


    public static class InfoTitle 
    {
        // учебные планы
        public static readonly string launch = "launch";
        public static readonly string login_elma = "login-elma";
        public static readonly string login_db = "login-db";
        public static readonly string start_upload = "start-upload";
        public static readonly string data_elma = "data-elma";
        public static readonly string data_db = "data-db";
        public static readonly string missed = "missed";
        public static readonly string inject_data = "inject-data";
        public static readonly string file_exists = "file-exists";
    }

    public static class WarnTItle 
    {
        public static readonly string fileNotFount = "not-found";
        public static readonly string directory = "directory";
    }

}