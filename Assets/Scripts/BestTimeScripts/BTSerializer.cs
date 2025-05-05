using System.Collections;
using System.Collections.Generic;

// This is writable to a file, and is a pure C# class. Takes all data provided and writes that data to a XML file
[System.Serializable]
public class BTSerializer
{
    // Runtime data structure that is represented by a Serialized XML file that is made for us
    public List<BestTime> list;
}

