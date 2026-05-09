namespace VSMC
{
    /// <summary>
    /// Used when an attachment has had its 'load in context' button pressed. 
    /// This will load the attachment as the main object, and either:
    ///     If the current shape exists as a backdrop, enable it and only it.
    ///     If not, add the current shape as a backdrop and enable it.
    /// </summary>
    public class LoadIntoAttachmentContext : LoadingContext
    {
        public string fullPathToShapeLoadedFrom;
        
        public LoadIntoAttachmentContext(string pathForOtherFile)
        {
            fullPathToShapeLoadedFrom = pathForOtherFile;
        }
    }
}