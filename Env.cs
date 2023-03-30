using System;
using System.Collections.Generic;
using System.IO;

namespace ELMA_API;

class EnvModule
{
    string fileName;
    string pathFile;
    List<EnvRecord> envsList = new List<EnvRecord>();

    public EnvModule(string fileName, bool log = true)
    {

        // * fileName have to start with dot ".", if there isn't dot then it will be
        // * automatically added 
        if (!fileName.StartsWith("."))
        {
            this.fileName = "." + fileName;
        }
        else
        {
            this.fileName = fileName;
        }

        this.pathFile = Path.Combine(Environment.CurrentDirectory, this.fileName);

        if (File.Exists(this.pathFile))
        {

            // log if true
            if (log)
                Log.Success(SuccessTitle.fileExists, "file \".env\" is found");

            string[] lines = File.ReadAllText(this.pathFile).Split("\n");

            foreach (var item in lines)
            {
                // check if line is empty or line doesn't include symbol =
                if (!String.IsNullOrEmpty(item) && item.IndexOf("=") != -1)
                {
                    string key = item.Substring(0, item.IndexOf("=")).Trim();
                    string value = item.Substring(item.IndexOf("=") + 1).Trim();

                    envsList.Add(new EnvRecord(key, value));
                }
            }
        }
        else
        {

            // log if true
            if (log)
            {
                Log.Warn(WarnTitle.fileNotFound, "file \".env\" is not exist");
                Log.Warn(WarnTitle.fileNotFound, $"{this.pathFile}");
            }

        }
    }

    public string getVar(string key)
    {
        foreach (EnvRecord record in this.envsList)
        {
            if (record.key == key)
            {
                return record.value;
            }
        }

        return "";
    }

}

class EnvRecord
{
    public string key;
    public string value;

    public EnvRecord(string key, string value)
    {
        this.key = key;
        this.value = value;
    }
}
    