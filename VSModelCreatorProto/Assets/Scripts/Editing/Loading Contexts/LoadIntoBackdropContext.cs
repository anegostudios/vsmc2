namespace VSMC
{
    /// <summary>
    /// Used when a backdrop has had its 'load in context' button pressed. 
    /// This will load the backdrop as the main object, and either:
    ///     If the current shape exists as an attachment, enable it. Leave other attachments as they are.
    ///     If not, add the current shape as an attachment and enable it too.
    /// </summary>
    public class LoadIntoBackdropContext : LoadingContext
    {
        public string fullPathToShapeLoadedFrom;

        public LoadIntoBackdropContext(string pathForOtherFile)
        {
            fullPathToShapeLoadedFrom = pathForOtherFile;
        }
    }
}