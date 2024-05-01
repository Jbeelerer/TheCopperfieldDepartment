using System.Collections.Generic;
using UnityEngine;

public interface IPinnable
{
    bool suspicious { get; }
    bool notSuspicious { get; }
}