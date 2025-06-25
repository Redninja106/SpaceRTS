using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Data;
internal abstract class AssetPrototype : DataPrototype
{
    public override void InitializePrototype()
    {
        base.InitializePrototype();
        LoadAssets(Prototypes.GetPrototypeDirectory(this.Name!));
    }

    public abstract void LoadAssets(string prototypeDirectory);
}
