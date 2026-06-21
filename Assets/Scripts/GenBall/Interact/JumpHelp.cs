using System;
using UnityEngine;

[Obsolete("Legacy component, to be removed after cleaning scene references in Prologue.unity")]
public class JumpHelp : MonoBehaviour
{
    public void PrintJumpHelp()
    {
        Debug.Log("Press Space to jump");
    }
}
