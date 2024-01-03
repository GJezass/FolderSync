using FolderSync;

public class Program
{
    //static ILogger GetConsoleLogger()
    //{
    //    return new ConsoleLogger
    //    {
    //        logType = LogType.Creation
    //    };
    //}

    static ILogger GetFileLogger(string path)
    {
        return new FileLogger(path) { 
            logType = LogType.Creation
        };
    }

    static void Main(string[] args)
    {
        // Check for the command-line arguments
        if (args.Length != 4)
        {
            Console.WriteLine("Missing arguments! Please provide the following arguments in order to run the program: \n\n-Source (Source folder path)\n-Replica (Target folder path)\n-logpath (Log file folder path)\n-Interval (In secs)\n");

            return;
        }

        // arguments
        string source = args[0];
        string target = args[1];
        string logpath = args[2];

        // parsing sync interval
        if (!int.TryParse(args[3], out int interval))
        {
            Console.WriteLine("Please provide a valid synchronization interval.");
            return;
        }

        var logger = GetFileLogger(logpath);

        try
        {
            // Run sync loop with interval
            while (true)
            {
                Console.WriteLine("");
                Console.WriteLine("******************************************");
                Console.WriteLine("");
                Console.WriteLine($"Syncing folder from {source} to {target} . . .");
                Console.WriteLine("");

                folderSync(source, target, logger);

                Console.WriteLine("");
                Console.WriteLine("//// Folder synchronization complete. ////");
                Console.WriteLine("");
                Thread.Sleep(interval * 1000); // convert to millis
            }
            
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Error encountered when synchronizing folders: {ex.Message}");
            return;
        }

    }

    #region methods

    /// <summary>
    /// Method for syncing files and subfolders (copying and removing)
    /// </summary>
    /// <param name="source_folder">Source folder</param>
    /// <param name="target_folder">Replica</param>
    /// <exception cref="DirectoryNotFoundException">If the source folder does not exist</exception>
    static void folderSync(string source_folder, string target_folder, ILogger logger)
    {
        // Source folder exists?
        if (!Directory.Exists(source_folder))
        {
            throw new DirectoryNotFoundException($"Source folder '{source_folder}' not found.");
        }

        // create target folder if it does not exist
        if (!Directory.Exists(target_folder))
        {
            Directory.CreateDirectory(target_folder);

            //logger.onLog += (logtype, message, logger) => { };

            logger.LogMessage(LogType.Creation, $"Target folder '{target_folder}' was created!");
            
        }

        // removing files from target
        FileRemove(target_folder, source_folder, logger);

        // removing subfolders from target
        SubRemove(target_folder, source_folder, logger);

        // sync files
        Directory.GetFiles(source_folder).ToList().ForEach(file =>
        {
            string source_filename = Path.GetFileName(file);
            string target_filepath = Path.Combine(target_folder, source_filename);

            // Checking if the file exists in the target
            bool fileExists = File.Exists(target_filepath);

            if (fileExists)
            {
                Console.WriteLine($"Updating '{source_filename}' on target folder...");
                logger.LogMessage(LogType.Copy, $"The file '{source_filename}' was updated on the target folder.");
            }
            else
            {
                Console.WriteLine($"Creating '{source_filename}' on target folder...");

                File.Copy(file, target_filepath, true);
                logger.LogMessage(LogType.Creation, $"The file '{source_filename}' was created on the target folder.");
            }

            
        });

        
        

        // recusive sync subfolders
        Directory.GetDirectories(source_folder).ToList().ForEach(sub =>
        {
            string sub_name = Path.GetFileName(sub);
            string target_sub = Path.Combine(target_folder,sub_name);

            Console.WriteLine($"Copying '{sub_name}' to target folder...");

            folderSync(sub, target_sub, logger);
            //logger.LogMessage(LogType.Copying, $"The subfolder '{sub_name}' was copied to the target folder.");
        });

    }

    /// <summary>
    /// Method for removing files from the target folder that no longer exist in the source folder
    /// </summary>
    /// <param name="targetFolder_path">replica path</param>
    /// <param name="sourceFolder_path">Source path</param>
    static void FileRemove(string targetFolder_path, string sourceFolder_path, ILogger logger)
    {
        // getting files from both folders
        string[] files_inTarget = Directory.GetFiles(targetFolder_path);
        string[] files_inSource = Directory.GetFiles(sourceFolder_path);

        // filtering the files that do not exist in both folders
        var files_toRemove = files_inTarget.Where(file_target => 
            !files_inSource.Any(file_source =>
                Path.GetFileName(file_target).Equals(Path.GetFileName(file_source), StringComparison.OrdinalIgnoreCase)
            )
        ).ToList();

        // Remove each file
        files_toRemove.ForEach(file =>
        {
            Console.WriteLine($"Deleting '{Path.GetFileName(file)}' from '{targetFolder_path}'...");
            File.Delete(file);
            logger.LogMessage(LogType.Removal, $"The file '{Path.GetFileName(file)}' was deleted from the target folder.");
        });
             

    }

    /// <summary>
    /// Method for removing subfolders from the target folder that no longer exist in the source folder
    /// </summary>
    /// <param name="targetFolder_path">replica path</param>
    /// <param name="sourceFolder_path">Source path</param>
    static void SubRemove(string targetFolder_path, string sourceFolder_path, ILogger logger)
    {
        // getting subfolders from both folders
        string[] subs_inTarget = Directory.GetDirectories(targetFolder_path);
        string[] subs_inSource = Directory.GetDirectories(sourceFolder_path);

        // filtering subs that do not exist in both folders
        var subs_toRemove = subs_inTarget.Where(sub_target =>
            !subs_inSource.Any(sub_source =>
                Path.GetFileName(sub_target).Equals(Path.GetFileName(sub_source), StringComparison.OrdinalIgnoreCase)
            )
        ).ToList();

        // Remove each subfolder and its contents
        subs_toRemove.ForEach(sub =>
        {
            Console.WriteLine($"Deleting subfolder '{Path.GetFileName(sub)}' from '{targetFolder_path}'...");

            Directory.Delete(sub, true);

            logger.LogMessage(LogType.Removal, $"The subfolder '{Path.GetFileName(sub)}' was deleted from the target folder.");
        });
    }

    #endregion

}

