﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal interface IDestructable
{
    bool IsDestroyed { get; }
    void OnDestroyed();
}
