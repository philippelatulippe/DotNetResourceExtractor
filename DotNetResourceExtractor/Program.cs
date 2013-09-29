/* Copyright (c) <2013>, Philippe Latulippe
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of the <ORGANIZATION> nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

/* Extracts files from .resources files.
 * 
 * This functionality is present in the Windows SDK tool resgen, but a bug 
 * causes it to crash on many files.
 * 
 * Can also extract resources from assemblies, as long as their resource file 
 * ends with .g.resources
 * 
 * .resources files can be extracted from assemblies using ILDasm
 * 
 * Requires .NET 4.0 because I used Stream.CopyTo()
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Collections;

namespace DotNetResourceExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Extracts resources from .net .resources files or assemblies containing *.g.resources files, dumps them in the working directory.");

            if (args.Count() != 1) {
                Console.WriteLine("Usage:  first argument should be a .resource file or an assembly.");
                return;
            }

            ResourceManager manager = null;

            //try{
                try{
                    Assembly assembly = Assembly.LoadFile(args[0]);

                    manager = new ResourceManager(assembly.GetName().Name + ".g", assembly);
                }catch(BadImageFormatException e){
                    // Ok, not an assembly.
                }

                if (manager == null){
                    manager = ResourceManager.CreateFileBasedResourceManager(Path.GetFileNameWithoutExtension(args[0]), Path.GetDirectoryName(args[0]), null);
                }

                //GetResourceSet() will return null unless I do a bogus fetch of a resource.  I probably need to RTFM more.
                try{
                    Object hello = manager.GetObject("buttwars");
                } catch (Exception) { }

                ResourceSet resourceset = manager.GetResourceSet(CultureInfo.InvariantCulture, false, false);

                foreach (DictionaryEntry resource in resourceset){
                    Console.WriteLine(resource.Key);

                    if (resource.Value is UnmanagedMemoryStream) {
                        UnmanagedMemoryStream stream = (UnmanagedMemoryStream)resource.Value;
                        String path = (String)resource.Key;
                        String directory = Path.GetDirectoryName(path);

                        if(path == null) {
                            Console.WriteLine("null?");
                        }

                        if (directory != null && directory.Length != 0) {
                            Directory.CreateDirectory(directory);
                        }

                        FileStream outputstream = File.Create(path);

                        stream.CopyTo(outputstream);

                        outputstream.Close();
                    }
                }

                Console.WriteLine(manager.ToString());
            /*}catch(Exception e){
                Console.Error.WriteLine(e.ToString());
            }*/
        }
    }
}
