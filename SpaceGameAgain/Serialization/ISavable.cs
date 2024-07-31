using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization;
internal interface ISavable
{
    void Save(Stream stream);
    void Load(Stream stream);
}
