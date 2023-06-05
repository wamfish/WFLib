//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

//ToDo: I don't think I will keep this around. At some point I will
//need to figure out how I handle exceptions and this will likely be
//refactored out of the codebase.

namespace WFLib;
public class PacketReadException : Exception
{
    public PacketReadException() { }
    public PacketReadException(string message) : base(message) { }
    public PacketReadException(string message, Exception inner) : base(message, inner) { }
}