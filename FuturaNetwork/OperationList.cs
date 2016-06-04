using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public class OperationList
    {
        public Stack<OperationModuleList> opList = new Stack<OperationModuleList>();
        public Stack<OperationModuleList> undoList = new Stack<OperationModuleList>();
        public void AddToOperation(OperationModuleList op)
        {
            //NetworkOperation op = new NetworkOperation();
            //op.oldObj = null;
            //op.newObj = new Node();
            //((Node)(op.newObj)).uid = "1";
            //_nodeElements.Add(((Node)(op.newObj)).uid, (Node)(op.newObj));
            //op.action = NetworkAction.Add;
            opList.Push(op);
            undoList.Clear();
        }

        public void ClearOperationList()
        {
            opList.Clear();
            undoList.Clear();
        }

        public void UndoOperationStack(Dictionary<string, Node> _nodeElements, Dictionary<string, Section> _sectionElements)
        {
            while (opList.Count != 0)
            {
                UndoOnce(_nodeElements, _sectionElements);
                //RedoOnce();
            }
        }

        //public void UndoModule(Dictionary<string, Node> _nodeElements, Dictionary<string, Section> _sectionElements, Dictionary<string, LinkObject> _nodeConnectivity)
        //{
        //    NetworkOperationModule op = opList.Pop();
        //    NetworkOperationModule opMod = ReverseModule(op);
        //    foreach (NetworkOperation undoOp in opMod)
        //    { 
        //    ProcessOp(undoOp, _nodeElements, _sectionElements, _nodeConnectivity);
        //    undoList.Push(undoOp);
        //}
        //}



        public void RedoOnce(Dictionary<string, Node> _nodeElements, Dictionary<string, Section> _sectionElements)
        {
            if (undoList.Count != 0)
            {
                OperationModuleList opModule = undoList.Pop();
                opModule = ReverseOp(opModule);
                ProcessOp(opModule, _nodeElements, _sectionElements);
                opList.Push(opModule);
            }
        }


        public void ProcessOp(OperationModuleList opModule, Dictionary<string, Node> _nodeElements, Dictionary<string, Section> _sectionElements)
        {
            if (opModule != null && opModule.operationModule != null && opModule.operationModule.Count > 0)
            {
                foreach (NetworkOperation op in opModule.operationModule)
                    ProcessOp(op, _nodeElements, _sectionElements);
            }
        }

        public void ProcessOp(NetworkOperation op, Dictionary<string, Node> _nodeElements, Dictionary<string, Section> _sectionElements)
        {
            switch (op.action)
            {
                case NetworkAction.Add:
                    {
                        if (typeof(Node) == op.newObj.GetType())
                        {
                            if (!_nodeElements.ContainsKey(((Node)op.newObj).uid))
                            {
                                _nodeElements.Add(((Node)op.newObj).uid, (Node)op.newObj);
                            }
                        }
                        else if (typeof(Section) == op.newObj.GetType())
                        {
                            Section newsect = ((Section)op.newObj);
                            if(_nodeElements.ContainsValue(newsect.parentNode))
                            {
                                Node pnd = newsect.parentNode;
                                if (!pnd.childList.ContainsKey(newsect.uid))
                                {
                                    pnd.childList.Add(newsect.uid, newsect);                                    
                                }
                                Node cnd = newsect.childNode;
                                if (!cnd.parentList.ContainsKey(newsect.uid))
                                {
                                    cnd.parentList.Add(newsect.uid, newsect);
                                }
                            }
                            _sectionElements.Add(((Section)op.newObj).uid, (Section)op.newObj);
                        }
                        break;
                    }
                case NetworkAction.Delete:
                    {
                        if (typeof(Node) == op.oldObj.GetType())
                        {
                            if (_nodeElements.ContainsKey(((Node)op.oldObj).uid))
                            {
                                Node nddel = ((Node)op.oldObj);
                                foreach (Section item in nddel.parentList.Values)
                                {
                                    item.childNode = null;
                                }
                                foreach (Section item in nddel.childList.Values)
                                {
                                    item.parentNode = null;
                                }
                                _nodeElements.Remove(((Node)op.oldObj).uid);
                            }
                        }
                        else if (typeof(Section) == op.oldObj.GetType())
                        {
                            Section oldsect = ((Section)op.oldObj);
                            if (_nodeElements.ContainsValue(oldsect.parentNode))
                            {
                                Node pnd = oldsect.parentNode;
                                if (pnd.childList.ContainsKey(oldsect.uid))
                                {
                                    pnd.childList.Remove(oldsect.uid);
                                }
                                Node cnd = oldsect.childNode;
                                if (cnd.parentList.ContainsKey(oldsect.uid))
                                {
                                    cnd.parentList.Remove(oldsect.uid);
                                }
                            }

                            if (_sectionElements.ContainsKey(((Section)op.oldObj).uid))
                                _sectionElements.Remove(((Section)op.oldObj).uid);
                        }                        
                        break;
                    }
                case NetworkAction.Update:
                    {
                        if (typeof(Node) == op.oldObj.GetType())
                        {
                            if (_nodeElements.ContainsKey(((Node)op.oldObj).uid))
                            {
                                Node newNode = (Node)(op.newObj);
                                _nodeElements[(((Node)op.oldObj).uid)] = (Node)(op.newObj);
                                Node ndOld = ((Node)op.oldObj);
                                foreach (Section item in ndOld.parentList.Values)
                                {
                                    item.childNode = newNode;
                                }
                                foreach (Section item in ndOld.childList.Values)
                                {
                                    item.parentNode = newNode;
                                }
                            }
                        }
                        else if (typeof(Section) == op.oldObj.GetType())
                        {
                            Section oldsect = ((Section)op.oldObj);
                            Section newSect = (Section)(op.newObj);
                            if (_nodeElements.ContainsValue(oldsect.parentNode))
                            {
                                Node pnd = oldsect.parentNode;
                                if (pnd.childList.ContainsKey(oldsect.uid))
                                {
                                    pnd.childList[oldsect.uid] = newSect;
                                }
                                Node cnd = oldsect.childNode;
                                if (cnd.parentList.ContainsKey(oldsect.uid))
                                {
                                    cnd.parentList[oldsect.uid] = newSect;
                                }
                            }
                            if (_sectionElements.ContainsKey(((Section)op.oldObj).uid))
                                _sectionElements[((Section)op.oldObj).uid] = (Section)(op.newObj);
                        }                       
                        break;
                    }
            }
        }

        public OperationModuleList ReverseOp(OperationModuleList opModule)
        {
            OperationModuleList revOpModule = new OperationModuleList();
            if (opModule != null && opModule.operationModule != null && opModule.operationModule.Count > 0)
            {
                opModule.operationModule.Reverse();
                foreach (NetworkOperation op in opModule.operationModule)
                {
                    NetworkOperation newOp = ReverseOp(op);
                    if (newOp != null) revOpModule.operationModule.Add(newOp);
                }
            }
            return revOpModule;
        }

        public NetworkOperation ReverseOp(NetworkOperation op)
        {
            NetworkOperation newOp = new NetworkOperation();
            switch (op.action)
            {
                case NetworkAction.Add:
                    {
                        newOp.action = NetworkAction.Delete;
                        newOp.oldObj = op.newObj;
                        newOp.newObj = null;
                        break;
                    }
                case NetworkAction.Delete:
                    {
                        newOp.action = NetworkAction.Add;
                        newOp.newObj = op.oldObj;
                        newOp.oldObj = null;
                        break;
                    }
                case NetworkAction.Update:
                    {
                        newOp.action = NetworkAction.Update;
                        newOp.newObj = op.oldObj;
                        newOp.oldObj = op.newObj;
                        break;
                    }
                case NetworkAction.None:
                    newOp.action = NetworkAction.None;
                    break;
            }
            return newOp;
        }

        public void UndoOnce(Dictionary<string, Node> _nodeElements, Dictionary<string, Section> _sectionElements)
        {
            OperationModuleList op = opList.Pop();
            OperationModuleList undoOp = ReverseOp(op);
            ProcessOp(undoOp, _nodeElements, _sectionElements);
            undoList.Push(undoOp);
        }
    }

}
