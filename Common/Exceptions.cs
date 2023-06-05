//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
//It is rare that you can recover gracefully from an exception, so I am not going to
//use very many. Mostly we will make sure we log them, and either exit or ignore depending
//on the context. Try to avoid adding more exceptions unless we have a reason to recover
//from it. 
public class WamfishException : Exception
{
    public WamfishException() : base("WFLib Exception") { }
    public WamfishException(string message) : base(message) { }
    public WamfishException(string message, Exception inner) : base(message, inner) { }
}
