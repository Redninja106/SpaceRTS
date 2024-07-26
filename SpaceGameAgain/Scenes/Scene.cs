using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Scenes;
internal  abstract class Scene
{
    public abstract void Tick();
    public abstract void Update(float tickProgress);
    public abstract void Render(ICanvas canvas);
}
