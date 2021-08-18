using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTable", menuName = "ScriptableObjects/Utilities/ZoneGeneratorReferenceTable", order = 1)]
public class ZoneGeneratorReferenceTable : InitializableRuntimeTable<string, ZoneGenerator>
{
}
