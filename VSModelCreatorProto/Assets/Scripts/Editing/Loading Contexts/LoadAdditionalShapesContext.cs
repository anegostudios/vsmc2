using System.Collections.Generic;

namespace VSMC
{
    
    public class LoadWithAdditionalShapesContext : LoadingContext
    {

        public List<AdditionalShapeData> newAdditionalShapes;
        public Dictionary<string, OriginPointData> rootElementOriginPoints;

        public LoadWithAdditionalShapesContext(List<AdditionalShapeData> newAdds, Dictionary<string, OriginPointData> rootOrigins)
        {
            this.newAdditionalShapes = newAdds;
            this.rootElementOriginPoints = rootOrigins;
        }

    }

}