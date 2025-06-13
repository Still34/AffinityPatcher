using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace AffinityPatcher.Utils;

public class DirectoryAccessGeneric
{
    public static bool HasWriteAccess(string directoryPath)
    {
        try
        {
            var tempFile = Path.Combine(directoryPath, Path.GetRandomFileName());
            using (var _ = File.Create(tempFile, 1, FileOptions.DeleteOnClose))
            {
            }
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}