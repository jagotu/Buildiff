using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff.FileFormats
{
    class CILFormat : StreamableFileFormat
    {
        public override bool CanLoad(Stream file)
        {
            try
            {
                AssemblyDefinition.ReadAssembly(file);
            } catch (BadImageFormatException)
            {
                return false;
            }
            return true;
        }

        private void CompareMethods(MethodDefinition oldMethod, MethodDefinition newMethod, ExportContext ec)
        {
            if (!oldMethod.HasBody)
            {
                return;
            }
            var oldInstructions = oldMethod.Body.Instructions;
            var newInstructions = newMethod.Body.Instructions;

            if (oldInstructions.Count != newInstructions.Count)
            {
                ec.ReportSelf(CompareResult.Modified);
                return;
            }

            for (int i = 0; i < oldInstructions.Count; i++)
            {
                if (oldInstructions[i].ToString() != newInstructions[i].ToString())
                {
                    ec.ReportSelf(CompareResult.Modified);
                    return;
                }


            }

            ec.ReportSelf(CompareResult.Identical);
        }

        private void CompareTypes(TypeDefinition oldType, TypeDefinition newType, ExportContext ec)
        {
            ec.Enter("Methods");
            DiffHelpers<MethodDefinition>.ThreeWayDiff(oldType.Methods, newType.Methods, x => x.FullName, CompareMethods, ec);
            ec.Leave();

            ec.Enter("Fields");
            DiffHelpers<FieldDefinition>.ThreeWayDiff(oldType.Fields, newType.Fields, x => x.FullName, ec);
            ec.Leave();
        }

        private void CompareResource(DictionaryEntry oldEntry, DictionaryEntry newEntry, ExportContext ec)
        {
            object oldVal = oldEntry.Value;
            object newVal = newEntry.Value;

            if(oldVal.GetType() != newVal.GetType())
            {
                ec.ReportSelf(CompareResult.Modified);
                return;
            }

            if(oldVal is Bitmap)
            {
                new ImageFormat().Compare((Bitmap)oldVal, (Bitmap)newVal, ec);
            }    
        }

        private void CompareResources(Resource oldRes, Resource newRes, ExportContext ec)
        {
            if (oldRes.ResourceType == ResourceType.Embedded && newRes.ResourceType == ResourceType.Embedded)
            {
                EmbeddedResource oldEmb = (EmbeddedResource)oldRes;
                EmbeddedResource newEmb = (EmbeddedResource)newRes;

                Stream oldStream = oldEmb.GetResourceStream();
                Stream newStream = newEmb.GetResourceStream();

                if (UnknownBinaryFormat.HashCompare(oldStream, newStream))
                {
                    ec.ReportSelf(CompareResult.Identical);
                }
                else
                {
                    oldStream.Seek(0, SeekOrigin.Begin);
                    newStream.Seek(0, SeekOrigin.Begin);
                    ResourceReader oldReader = new ResourceReader(oldStream);
                    ResourceReader newReader = new ResourceReader(newStream);

                    DiffHelpers<DictionaryEntry>.ThreeWayDiff(oldReader.Cast<DictionaryEntry>(), newReader.Cast<DictionaryEntry>(), x => x.Key.ToString(), CompareResource, ec);
                }
            }
        }

        private void CompareModules(ModuleDefinition oldModule, ModuleDefinition newModule, ExportContext ec)
        {
            ec.Enter("Types");
            DiffHelpers<TypeDefinition>.ThreeWayDiff(oldModule.Types, newModule.Types, x => x.FullName, CompareTypes, ec);
            ec.Leave();

            ec.Enter("Resources");
            DiffHelpers<Resource>.ThreeWayDiff(oldModule.Resources, newModule.Resources, x => x.Name, CompareResources, ec);
            ec.Leave();
        }

        public override void Compare(Stream oldFile, Stream newFile, ExportContext ec)
        {
            AssemblyDefinition oldAss = AssemblyDefinition.ReadAssembly(oldFile);
            AssemblyDefinition newAss = AssemblyDefinition.ReadAssembly(newFile);

            

            DiffHelpers<ModuleDefinition>.ThreeWayDiff(oldAss.Modules, newAss.Modules, x => x.Name, CompareModules, ec);
        }
    }
}
