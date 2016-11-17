using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace MySecondAddin
{
    public class Class1
    {
        [Autodesk.AutoCAD.Runtime.CommandMethod("SamCommand")]
        public void SamCommand()
        {
            System.Windows.Forms.MessageBox.Show("Whatever you do do not type hello world!");
        }

        [Autodesk.AutoCAD.Runtime.CommandMethod("SamCommand2", Autodesk.AutoCAD.Runtime.CommandFlags.Session)]
        public void SamCommand2()
        {
            MessageBox.Show("Session Flag means the command can cross multiple documents.");
        }

        [Autodesk.AutoCAD.Runtime.CommandMethod("SamCommand3", Autodesk.AutoCAD.Runtime.CommandFlags.UsePickSet)]
        public void SamCommand3()
        {
            Autodesk.AutoCAD.ApplicationServices.Document myDoc =
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.EditorInput.Editor myEd = myDoc.Editor;
            Autodesk.AutoCAD.EditorInput.PromptSelectionResult myPSR = myEd.SelectImplied();
            if (myPSR.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                System.Windows.Forms.MessageBox.Show(myPSR.Value.Count.ToString() + " selected.");
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("0 selected.");
            }
        }

        [Autodesk.AutoCAD.Runtime.CommandMethod("SamCommand4")]
        public void SamCommand4()
        {
            Autodesk.AutoCAD.DatabaseServices.Database myDB;
            myDB = Autodesk.AutoCAD.DatabaseServices.HostApplicationServices.WorkingDatabase;
            using (Autodesk.AutoCAD.DatabaseServices.Transaction myTrans =
            myDB.TransactionManager.StartTransaction())
            {
                Autodesk.AutoCAD.Geometry.Point3d startPoint =
                new Autodesk.AutoCAD.Geometry.Point3d(1, 2, 3);
                Autodesk.AutoCAD.Geometry.Point3d endPoint =
                new Autodesk.AutoCAD.Geometry.Point3d(4, 5, 6);
                Autodesk.AutoCAD.DatabaseServices.Line myLine =
                new Autodesk.AutoCAD.DatabaseServices.Line(startPoint, endPoint);
                Autodesk.AutoCAD.DatabaseServices.BlockTableRecord myBTR =
                (Autodesk.AutoCAD.DatabaseServices.BlockTableRecord)
                myDB.CurrentSpaceId.GetObject(Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                myBTR.AppendEntity(myLine);
                myTrans.AddNewlyCreatedDBObject(myLine, true);
                myTrans.Commit();
            }
        }

        [CommandMethod("SamCommand5")]
        public void SamCommand5()
        {
            Database myDB;
            myDB = HostApplicationServices.WorkingDatabase;
            using (Transaction myTrans = myDB.TransactionManager.StartTransaction())
            {
                Point3d startPoint = new Point3d(1, 2, 3);
                Point3d endPoint = new Point3d(4, 5, 6);
                Line myLine = new Line(startPoint, endPoint);
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                myBTR.AppendEntity(myLine);
                myTrans.AddNewlyCreatedDBObject(myLine, true);
                myTrans.Commit();
            }
        }
        [CommandMethod("SamCommand6")]
        public void Command6()
        {
            Database myDB;
            myDB = HostApplicationServices.WorkingDatabase;
            using (Transaction myTrans = myDB.TransactionManager.StartTransaction())
            {
                Autodesk.AutoCAD.EditorInput.Editor myEd =
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                Point3d startPoint = myEd.GetPoint("First Point:").Value;
                Point3d endPoint = myEd.GetPoint("Second Point:").Value;
                Line myLine = new Line(startPoint, endPoint);
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                myBTR.AppendEntity(myLine);
                myTrans.AddNewlyCreatedDBObject(myLine, true);
                myTrans.Commit();
            }
        }

        [CommandMethod("SamCommand7")]
        public void SamCommand7()
        {
            System.IO.FileInfo myFIO = new System.IO.FileInfo("C:\\temp\\blocks.txt");
            if (myFIO.Directory.Exists == false)
            {
                myFIO.Directory.Create();
            }
            Database dbToUse = HostApplicationServices.WorkingDatabase;
            System.IO.StreamWriter mySW = new System.IO.StreamWriter(myFIO.FullName);
            mySW.WriteLine(HostApplicationServices.WorkingDatabase.Filename);
            foreach (string myName in GetBlockNames(dbToUse))
            {
                foreach (ObjectId myBrefID in GetBlockIDs(dbToUse, myName))
                {
                    mySW.WriteLine(" " + myName);
                    foreach (KeyValuePair<string, string> myKVP in GetAttributes(myBrefID))
                    {
                        mySW.WriteLine(" " + myKVP.Key + " " + myKVP.Value);
                    }
                }
            }
            mySW.Close();
            mySW.Dispose();
        }


        List<string> GetBlockNames(Database DBIn)
        {
            List<string> retList = new List<string>();
            using (Transaction myTrans = DBIn.TransactionManager.StartTransaction())
            {
                BlockTable myBT = (BlockTable)DBIn.BlockTableId.GetObject(OpenMode.ForRead);
                foreach (ObjectId myOID in myBT)
                {
                    BlockTableRecord myBTR = (BlockTableRecord)myOID.GetObject(OpenMode.ForRead);
                    if (myBTR.IsLayout == false | myBTR.IsAnonymous == false)
                    {
                        retList.Add(myBTR.Name);
                    }
                }
            }
            return (retList);
        }

        ObjectIdCollection GetBlockIDs(Database DBIn, string BlockName)
        {
            ObjectIdCollection retCollection = new ObjectIdCollection();
            using (Transaction myTrans = DBIn.TransactionManager.StartTransaction())
            {
                BlockTable myBT = (BlockTable)DBIn.BlockTableId.GetObject(OpenMode.ForRead);
                if (myBT.Has(BlockName))
                {
                    BlockTableRecord myBTR = (BlockTableRecord)myBT[BlockName].GetObject(OpenMode.ForRead);
                    retCollection = (ObjectIdCollection)myBTR.GetBlockReferenceIds(true, true);
                    myTrans.Commit();
                    return (retCollection);
                }
                else
                {
                    myTrans.Commit();
                    return (retCollection);
                }
            }
        }

        Dictionary<string, string> GetAttributes(ObjectId BlockRefID)
        {
            Dictionary<string, string> retDictionary = new Dictionary<string, string>();
            using (Transaction myTrans = BlockRefID.Database.TransactionManager.StartTransaction())
            {
                BlockReference myBref = (BlockReference)BlockRefID.GetObject(OpenMode.ForRead);
                if (myBref.AttributeCollection.Count == 0)
                {
                    return (retDictionary);
                }
                else
                {
                    foreach (ObjectId myBRefID in myBref.AttributeCollection)
                    {
                        AttributeReference myAttRef = (AttributeReference)myBRefID.GetObject(OpenMode.ForRead);
                        if (retDictionary.ContainsKey(myAttRef.Tag) == false)
                        {
                            retDictionary.Add(myAttRef.Tag, myAttRef.TextString);
                        }
                    }
                    return (retDictionary);
                }
            }
        }

    }
}
