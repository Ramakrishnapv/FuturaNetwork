using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futura.ArcGIS.NetworkExtension
{
    public class GISApplication
    {

        /// <summary>
        /// Checks to see if the workspace is being edited.
        /// </summary>
        /// <param name="workSpace">The workspace for which to check the edit state.</param>
        /// <returns>Return true if the workspace is being edited, otherwise returns false.</returns>
        public static bool IsBeingEdited(IWorkspace workSpace)
        {
            bool isBeingEdited = false;
            if (workSpace != null)
            {
                if (workSpace is IWorkspaceEdit)
                    isBeingEdited = ((IWorkspaceEdit)workSpace).IsBeingEdited();
            }
            return isBeingEdited;
        }


        public static bool SnapPoint(IPoint point)
        {
            bool successfulSnap = false;

            IPoint pt = point as IPoint;

            if (pt != null)
            {
                if (ExtensionInfo.Editor != null)
                {
                    ISnapEnvironment snapEnvironment = ExtensionInfo.Editor as ISnapEnvironment;
                    if (snapEnvironment != null) successfulSnap = snapEnvironment.SnapPoint(pt);
                }
            }
            return successfulSnap;
        }

        /// <summary>
        /// Checks to see if the workspace is in an edit operation.
        /// </summary>
        /// <param name="workSpace">The workspace for which to check for an edit operation.</param>
        /// <returns>Returns true if the workspace is in an edit operation, otherwise returns false.</returns>
        public static bool IsInEditOperation(IWorkspace workSpace)
        {
            if (workSpace != null)
            {
                IWorkspaceEdit2 wsEdit = workSpace as IWorkspaceEdit2;
                if (wsEdit != null)
                    return wsEdit.IsInEditOperation;
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the workspace has pending edits.
        /// </summary>
        /// <param name="workSpace">The workspace for which to check for edits.</param>
        /// <returns>Returns true if the workspace has pending edits, otherwise returns false.</returns>
        public static bool HasEdits(IWorkspace workSpace)
        {
            bool hasEdits = false;
                IWorkspaceEdit wsEdit = workSpace as IWorkspaceEdit;
                if (wsEdit != null)
                    wsEdit.HasEdits(ref hasEdits);
            return hasEdits;
        }

        /// <summary>
        /// Starts an edit session.
        /// </summary>
        /// <param name="workSpace">The workspace for which to start the edit session.</param>
        /// <param name="enableUndoRedo">Boolean determining whether or not to enable undo/redo.</param>
        /// <returns>Returns true if the edit session was successfully started, otherwise returns false.</returns>
        private static bool StartEditing(IWorkspace workSpace, bool enableUndoRedo)
        {
            bool sessionStarted = false;
            if (workSpace != null)
            {
                try
                {
                    if (workSpace is IWorkspaceEdit)
                    {
                        IWorkspaceEdit workSpaceEdit = (IWorkspaceEdit)workSpace;
                        workSpaceEdit.StartEditing(enableUndoRedo);
                        sessionStarted = workSpaceEdit.IsBeingEdited();
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogAndDisplayException(e);
                }
            }
            return sessionStarted;
        }

        /// <summary>
        /// Stops an edit session.
        /// </summary>
        /// <param name="workSpace">The workspace for which to stop the edit session.</param>
        /// <param name="saveEdits">Boolean determining whether or not to save edits.</param>
        /// <returns>Returns true if the edit session was successfully stopped, otherwise returns false.</returns>
        private static bool StopEditing(IWorkspace workSpace, bool saveEdits)
        {
            bool sessionEnded = false;
            if (workSpace != null)
            {
                try
                {
                    if (workSpace is IWorkspaceEdit)
                    {
                        IWorkspaceEdit workSpaceEdit = (IWorkspaceEdit)workSpace;
                        workSpaceEdit.StopEditing(saveEdits);
                        sessionEnded = !workSpaceEdit.IsBeingEdited();
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogAndDisplayException(e);
                }
            }
            return sessionEnded;
        }

        /// <summary>
        /// Starts an edit operation.
        /// </summary>
        /// <param name="workSpace">The workspace for which to start the edit operation.</param>
        /// <returns>Returns true if the edit operation was successfully started, otherwise returns false.</returns>
        private static bool StartOperation(IWorkspace workSpace)
        {
            bool opStarted = false;
            if (workSpace != null)
            {
                if (workSpace is IWorkspaceEdit)
                {
                    if (IsBeingEdited(workSpace))
                    {
                        IWorkspaceEdit workSpaceEdit = (IWorkspaceEdit)workSpace;
                        try
                        {
                            workSpaceEdit.StartEditOperation();
                            opStarted = true;
                        }
                        catch (Exception e)
                        {
                           // _logger.LogAndDisplayException(e);
                        }
                    }
                }
            }
            return opStarted;
        }

        /// <summary>
        /// Stops an edit operation.
        /// </summary>
        /// <param name="workSpace">The workspace for which to stop the edit operation.</param>
        /// <returns>Returns true if the edit operation was successfully stopped, otherwise returns false.</returns>
        private static bool StopOperation(IWorkspace workSpace)
        {
            bool opStop = false;
            if (workSpace != null)
            {
                if (workSpace is IWorkspaceEdit)
                {
                    IWorkspaceEdit workSpaceEdit = (IWorkspaceEdit)workSpace;
                    try
                    {
                        workSpaceEdit.StopEditOperation();
                        opStop = true;
                    }
                    catch (Exception e)
                    {
                        //_logger.LogAndDisplayException(e);
                    }
                }
            }
            return opStop;
        }

        /// <summary>
        /// Aborts the current edit operation.
        /// </summary>
        /// <param name="workSpace">The workspace for which to abort the edit operation.</param>
        /// <returns>Returns true if the edit operation was successfully aborted, otherwise returns false.</returns>
        private static bool AbortOperation(IWorkspace workSpace)
        {
            bool aborted = false;
            if (workSpace != null)
            {
                if (workSpace is IWorkspaceEdit)
                {
                    IWorkspaceEdit workSpaceEdit = (IWorkspaceEdit)workSpace;
                    try
                    {
                        workSpaceEdit.AbortEditOperation();
                        aborted = true;
                    }
                    catch (Exception e)
                    {
                        //_logger.LogAndDisplayException(e);
                    }
                }
            }
            return aborted;
        }

        /// <summary>
        /// Aborts the current edit operation.
        /// </summary>
        /// <param name="workSpace">The workspace for which to abort the edit operation.</param>
        public static void AbortEditOperation(IWorkspace workSpace)
        {
            AbortOperation(workSpace);
        }

        /// <summary>
        /// Stops an edit operation.
        /// </summary>
        /// <param name="workSpace">The workspace for which to stop the edit operation.</param>
        /// <returns>Returns true if the edit operation was successfully stopped, otherwise returns false.</returns>
        public static bool StopEditOperation(IWorkspace workSpace)
        {
            bool opStop = false;
 
                    opStop = StopOperation(workSpace);
       
            return opStop;
        }

        /// <summary>
        /// Starts an edit operation.
        /// </summary>
        /// <param name="workSpace">The workspace for which to start the edit operation.</param>
        /// <returns>Returns true if the edit operation was successfully started, otherwise returns false.</returns>
        public static bool StartEditOperation(IWorkspace workSpace)
        {
            bool opStart = false;
           
                    opStart = StartOperation(workSpace);
       
            return opStart;
        }

        /// <summary>
        /// Starts an edit session.
        /// </summary>
        /// <param name="workSpace">The workspace for which to start the edit session.</param>
        /// <returns>Returns true if the edit session was successfully started, otherwise returns false.</returns>
        public static bool StartEditSession(IWorkspace workSpace)
        {
            bool sessionStart = false;

                    sessionStart = StartEditing(workSpace, true);
    
            return sessionStart;
        }


        /// <summary>
        /// Starts an Versioned/Non-Versioned edit session based on the input Dataset. If it is Versioned Dataset, 
        /// starts Versioned Edit session, if it is Non-versioned Dataset, starts Non-versioned edit session.
        /// </summary>
        /// <param name="workSpace">The workspace for which to start the edit session.</param>
        /// <param name="dataset">The dataset to be edited.</param>
        /// <returns>Returns true if the edit session was successfully started, otherwise returns false.</returns>
        public static bool StartEditSession(IWorkspace workSpace, IDataset dataset)
        {
            bool sessionStart = false;

            if (workSpace == null || dataset == null) return false;

            try
            {
                if (workSpace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                {
                    IVersionedObject versionedObject = dataset as IVersionedObject;
                    if (versionedObject != null && versionedObject.IsRegisteredAsVersioned == false)
                        sessionStart = StartNonVersionedEditSession(workSpace);
                }
                if (!sessionStart)
                    sessionStart = StartEditSession(workSpace);
            }
            catch (Exception e)
            {
                //_logger.LogException(e);
            }

            return sessionStart;
        }

        /// <summary>
        /// Starts an edit session for editing Non-Versioned data.
        /// </summary>
        /// <param name="workSpace">The workspace for which to start the edit session.</param>
        /// <returns>Returns true if the edit session was successfully started, otherwise returns false.</returns>
        public static bool StartNonVersionedEditSession(IWorkspace workSpace)
        {
            bool sessionStart = false;

            if (workSpace != null)
            {
                try
                {
                    if (workSpace is IWorkspaceEdit)
                    {
                        IWorkspaceEdit workSpaceEdit = (IWorkspaceEdit)workSpace;
                        if (workSpace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                        {
                            IMultiuserWorkspaceEdit muWorkspaceEdit = (IMultiuserWorkspaceEdit)workSpace;

                            // Make sure that non-versioned editing is supported. If not, throw an exception.
                            if (muWorkspaceEdit != null && muWorkspaceEdit.SupportsMultiuserEditSessionMode
                                                        (esriMultiuserEditSessionMode.esriMESMNonVersioned))
                            {
                                // Start a non-versioned edit session.
                                muWorkspaceEdit.StartMultiuserEditing
                                  (esriMultiuserEditSessionMode.esriMESMNonVersioned);
                                sessionStart = true;
                            }
                        }

                        if (!sessionStart)
                            StartEditSession(workSpace);

                        sessionStart = workSpaceEdit.IsBeingEdited();
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogAndDisplayException(e);
                }
            }

            return sessionStart;
        }
        /// <summary>
        /// Stops an edit session.
        /// </summary>
        /// <param name="workSpace">The workspace for which to stop the edit session.</param>
        /// <param name="saveEdits">Boolean determining whether or not to save edits.</param>
        /// <returns>Returns true if the edit session was successfully stopped, otherwise returns false.</returns>
        public static bool StopEditSession(IWorkspace workSpace, bool saveEdits)
        {
            bool sessionStop = false;
 
                    sessionStop = StopEditing(workSpace, saveEdits);
     
            return sessionStop;
        }
    }
}
