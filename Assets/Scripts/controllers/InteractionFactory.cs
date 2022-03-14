using System.Collections.Generic;
using UnityEngine;

public static class InteractionFactory {

    // ***** Grabs

    // * [Grab] Plain grab in Grab mode, just move top level entity

    public static bool TryStartPlainGrab(Transform target, GrabSource source) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.grabbable) {
            SceneEvents.Warn("Not grabbable (static)", proc.transform);
            return false;
        }

        Grabber.instance.EnableUserLocks();
        Grabber.instance.StartGrab(target, source);
        Grabber.instance.CallOnRelease(ReleasePlainGrab);
        Grabber.instance.CallOnCancel(CancelPlainGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleasePlainGrab(Transform target) {
        Grabber.instance.SaveAllGrabs();
        UndoHistory.current.Commit();
        Metrics.current.ReleaseGrab(PMode.GlobalGrab);
        SceneEvents.Logging("PlainGrab", target);
    }
    public static void CancelPlainGrab(Transform target) {
        UndoHistory.current.Discard();
        Metrics.current.CancelGrab();
    }

    // * [Clone] Clone grab in Clone mode, clone entity and move it

    public static bool TryStartCloneGrab(Transform target, GrabSource source) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.cloneable) {
            SceneEvents.Warn("Not cloneable (static)", proc.transform);
            return false;
        }

        var clone = ProceduralFactory.Clone(target);
        Grabber.instance.EnableUserLocks();
        Grabber.instance.StartGrab(clone, source);
        Grabber.instance.CallOnRelease(ReleaseCloneGrab);
        Grabber.instance.CallOnCancel(CancelCloneGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleaseCloneGrab(Transform clone) {
        Grabber.instance.SaveAllGrabs();
        UndoHistory.current.Commit();
        Metrics.current.ReleaseGrab(PMode.GlobalClone);
        SceneEvents.Logging("PlainGrab", clone);
    }
    public static void CancelCloneGrab() {
        UndoHistory.current.Discard();
        Metrics.current.CancelClone();
    }

    // * [Edit>Group>Grab] Grab child of group entity in Edit>Group

    public static bool TryStartGroupMoveGrab(Transform tail, Transform target, GrabSource source) {
        if (!Procedural.IsProcedural(target)) {
            SceneEvents.InternalError("Not a procedural", target);
            return false;
        }
        if (target.parent != tail) {
            SceneEvents.Error("Can only move children of the Group", tail);
            return false;
        }

        var procGroup = tail.GetComponent<ProceduralGroup>();
        procGroup.InitializeMoveGrab(target);
        Grabber.instance.StartGrab(target, source);
        Grabber.instance.CallOnRelease(ReleaseGroupMoveGrab);
        Grabber.instance.CallOnCancel(CancelGroupMoveGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleaseGroupMoveGrab(Transform target) {
        Grabber.instance.SaveAllGrabs();
        UndoHistory.current.Commit();
        Metrics.current.ReleaseGrab(PMode.EditGroupGrab);
        SceneEvents.Logging("GroupMove", target);
    }
    public static void CancelGroupMoveGrab(Transform target) {
        UndoHistory.current.Discard();
        Metrics.current.CancelGrab();
    }

    // * [Edit>Group>Clone] Grab clone for group entity in Edit>Group

    public static bool TryStartGroupCloneGrab(Transform tail, Transform target, GrabSource source) {
        var proc = target.GetComponent<Procedural>();
        if (target.parent != tail && !Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        Transform blocker;
        if (LinkedProcedural.Bound(tail, target, out blocker)) {
            SceneEvents.Error("That would create a cycle", blocker);
            return false;
        }

        var procGroup = tail.GetComponent<ProceduralGroup>();
        var clone = procGroup.InitializeCloneGrab(target);
        Grabber.instance.StartGrab(clone, source);
        Grabber.instance.CallOnRelease(ReleaseGroupCloneGrab);
        Grabber.instance.CallOnCancel(CancelGroupCloneGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleaseGroupCloneGrab(Transform clone) {
        Grabber.instance.SaveAllGrabs();
        UndoHistory.current.Commit();
        Metrics.current.ReleaseGrab(PMode.EditGroupClone);
        Metrics.current.UpdateLargest(clone.parent);
        SceneEvents.Logging("GroupClone", clone);
    }
    public static void CancelGroupCloneGrab() {
        UndoHistory.current.Discard();
        Metrics.current.CancelClone();
    }

    // * [Edit>Random>Grab] Grab child of random entity in Edit>Random

    public static bool TryStartRandomMoveGrab(Transform tail, Transform target, GrabSource source) {
        if (!Procedural.IsProcedural(target)) {
            SceneEvents.InternalError("Not a procedural", target);
            return false;
        }
        if (target.parent != tail) {
            SceneEvents.Error("Can only move the variant of the Random", tail);
            return false;
        }

        var procRandom = tail.GetComponent<ProceduralRandom>();
        procRandom.InitializeMoveGrab(target);
        Grabber.instance.StartGrab(target, source);
        Grabber.instance.CallOnRelease(ReleaseGroupMoveGrab);
        Grabber.instance.CallOnCancel(CancelGroupMoveGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleaseRandomMoveGrab(Transform target) {
        Grabber.instance.SaveAllGrabs();
        UndoHistory.current.Commit();
        Metrics.current.ReleaseGrab(PMode.EditRandomGrab);
        SceneEvents.Logging("RandomMove", target);
    }
    public static void CancelRandomMoveGrab(Transform target) {
        UndoHistory.current.Discard();
        Metrics.current.CancelGrab();
    }

    // * [Edit>Random>Clone] Clone child of random entity in Edit>Random

    public static bool TryStartRandomCloneGrab(Transform tail, Transform target, GrabSource source) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        Transform blocker;
        if (LinkedProcedural.Bound(tail, target, out blocker)) {
            SceneEvents.Error("That would create a cycle", blocker);
            return false;
        }

        var procRandom = tail.GetComponent<ProceduralRandom>();
        var clone = procRandom.InitializeCloneGrab(target);
        Grabber.instance.StartGrab(clone, source);
        Grabber.instance.CallOnRelease(ReleaseRandomCloneGrab);
        Grabber.instance.CallOnCancel(CancelRandomCloneGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleaseRandomCloneGrab(Transform clone) {
        Grabber.instance.SaveAllGrabs();
        foreach (Transform random in LinkedProcedural.GetLinks(clone.parent, false)) {
            ProceduralFactory.Randomize(random);
        }
        UndoHistory.current.Commit();
        Metrics.current.ReleaseGrab(PMode.EditRandomClone);
        Metrics.current.UpdateLargest(clone.parent);
        SceneEvents.Logging("RandomClone", clone);
    }
    public static void CancelRandomCloneGrab() {
        UndoHistory.current.Discard();
        Metrics.current.CancelClone();
    }

    // * [Edit>Position>Grab] Grab the handle in Edit>Position

    public static bool TryStartPositionGrab(Transform tail, Transform target, GrabSource source) {
        var proc = tail.GetComponent<ProceduralPosition>();
        if (target.parent != tail) {
            SceneEvents.Warn("Can only move the handle", proc.handle);
            return false;
        }

        target = proc.InitializeGrab(target);
        if (target == null) {
            SceneEvents.Warn("Move the handle instead", proc.handle);
            return false;
        }

        Grabber.instance.StartGrab(target, source);
        Grabber.instance.ProjectOnLaser();
        Grabber.instance.CallOnRelease((target) => ReleasePositionGrab(tail, target));
        Grabber.instance.CallOnCancel(CancelPositionGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleasePositionGrab(Transform tail, Transform target) {
        Grabber.instance.SaveAllGrabs();
        RefreshAllLinksProceduralPosition(tail);
        Metrics.current.ReleaseGrab(PMode.EditPositionGrab);
        SceneEvents.Logging("PositionGrab", target);
    }
    public static void CancelPositionGrab(Transform target) {
        UndoHistory.current.Discard();
        Metrics.current.CancelGrab();
    }

    // * [Edit>Rotation>Grab] Grab one of the handles in Edit>Rotation
    public static bool TryStartRotationGrab(Transform tail, Transform target, GrabSource source) {
        var proc = tail.GetComponent<ProceduralRotation>();
        if (target.parent != tail) {
            SceneEvents.Info("Can only move the handles", proc.pivot);
            return false;
        }

        target = proc.InitializeGrab(target);
        if (target == null) {
            SceneEvents.Info("Can only move the handles", proc.pivot);
            return false;
        }

        Grabber.instance.StartGrab(target, source);
        Grabber.instance.ProjectOnLaser();
        Grabber.instance.CallOnRelease((target) => ReleaseRotationGrab(tail, target));
        Grabber.instance.CallOnCancel(CancelRotationGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleaseRotationGrab(Transform tail, Transform target) {
        Grabber.instance.SaveAllGrabs();
        RefreshAllLinksProceduralRotation(tail);
        Metrics.current.ReleaseGrab(PMode.EditRotationGrab);
        SceneEvents.Logging("RotationGrab", target);
    }
    public static void CancelRotationGrab(Transform target) {
        UndoHistory.current.Discard();
        Metrics.current.CancelGrab();
    }

    // * [Edit>Tiling>Grab] Grab one of the pilars or the bell in Edit>Tiling
    public static bool TryStartTilingGrab(Transform tail, Transform target, GrabSource source) {
        var proc = tail.GetComponent<ProceduralTiling>();
        if (target.parent != tail) {
            SceneEvents.Info("Can only move the handles", proc.augments);
            return false;
        }

        target = proc.InitializeGrab(target);
        if (target == null) {
            SceneEvents.Info("Can only move the handles", proc.augments);
            return false;
        }

        Grabber.instance.StartGrab(target, source);
        Grabber.instance.ProjectOnLaser();
        Grabber.instance.CallOnRelease(ReleaseTilingGrab);
        Grabber.instance.CallOnCancel(CancelTilingGrab);
        Metrics.current.StartGrab();
        return true;
    }
    public static void ReleaseTilingGrab(Transform target) {
        Grabber.instance.SaveAllGrabs();
        UndoHistory.current.Commit();
        Metrics.current.ReleaseGrab(PMode.EditTilingGrab);
        SceneEvents.Logging("TilingGrab", target);
    }
    public static void CancelTilingGrab(Transform target) {
        UndoHistory.current.Discard();
        Metrics.current.CancelGrab();
    }

    // ***** Individual operations in Edit mode

    // Try to delete an element of a group, refuse to do so if it is the last remaining
    public static bool TryDeleteGroupChild(Transform tail, Transform target) {
        var procGroup = tail.GetComponent<ProceduralGroup>();
        if (procGroup == null) {
            SceneEvents.InternalError("Not a Group", tail);
            return false;
        }
        if (target.parent != tail) {
            SceneEvents.Info("Can only delete elements of Group", tail);
            return false;
        }
        if (Procedural.CountProceduralChildren(tail) == 1) {
            SceneEvents.Error("Can't delete the last element", target);
            return false;
        }

        var associates = LinkedProcedural.GetAssociates(target, true);
        ProceduralFactory.LinkedDelete(associates);
        UndoHistory.current.Commit();
        SceneEvents.DeleteChild(tail);
        Metrics.current.AddAction(PAction.GroupDeleteChild);
        return true;
    }

    // Try to delete a variant of a random, refuse to do so if it is the last remaining
    public static bool TryDeleteRandomChild(Transform tail, Transform target) {
        var procRandom = tail.GetComponent<ProceduralRandom>();
        if (procRandom == null) {
            SceneEvents.InternalError("Not a Random", tail);
            return false;
        }
        if (!target.gameObject.activeSelf) {
            SceneEvents.InternalError("Not active?", tail);
            return false;
        }
        if (target.parent != tail) {
            SceneEvents.Info("Can only delete variants of Random", tail);
            return false;
        }
        if (Procedural.CountProceduralChildren(tail) == 1) {
            SceneEvents.Error("Can't delete the last variant", target);
            return false;
        }

        var associates = LinkedProcedural.GetAssociates(target, true);
        foreach (var associate in associates) {
            var random = associate.parent.GetComponent<ProceduralRandom>();
            if (random.activeChild == associate) {
                random.CycleSaved();
            }
        }
        // If this random's active child is an empty, then show empties
        if (procRandom.activeProc is ProceduralEmpty) {
            ProceduralEmpty.SetStateAll(true);
        }
        ProceduralFactory.LinkedDelete(associates);
        UndoHistory.current.Commit();
        SceneEvents.DeleteChild(tail);
        Metrics.current.AddAction(PAction.RandomDeleteChild);
        return true;
    }

    // Cycle a random to the next variant
    public static bool CycleRandomChild(Transform tail) {
        var procRandom = tail.GetComponent<ProceduralRandom>();
        if (procRandom == null) {
            SceneEvents.InternalError("Not a Random", tail);
            return false;
        }

        procRandom.CycleSaved();
        if (procRandom.activeProc is ProceduralEmpty) {
            ProceduralEmpty.SetStateAll(true);
        }
        UndoHistory.current.Commit();
        SceneEvents.Cycle(tail);
        Metrics.current.AddAction(PAction.RandomCycle);
        return true;
    }

    public static bool RefreshProceduralPosition(Transform tail) {
        var procPosition = tail.GetComponent<ProceduralPosition>();
        if (procPosition == null) {
            SceneEvents.InternalError("Not a Mover", tail);
            return false;
        }

        procPosition.RefreshSaved();
        UndoHistory.current.Commit();
        SceneEvents.Refresh(Procedural.FirstProceduralChild(tail, true));
        Metrics.current.AddAction(PAction.PositionRefresh);
        return true;
    }

    public static bool RefreshAllLinksProceduralPosition(Transform tail) {
        var procPosition = tail.GetComponent<ProceduralPosition>();
        if (procPosition == null) {
            SceneEvents.InternalError("Not a Mover", tail);
            return false;
        }

        foreach (Transform link in LinkedProcedural.GetLinks(tail)) {
            var proc = link.GetComponent<ProceduralPosition>();
            proc.RefreshSaved();
        }
        UndoHistory.current.Commit();
        SceneEvents.Refresh(Procedural.FirstProceduralChild(tail, true));
        Metrics.current.AddAction(PAction.PositionRefresh);
        return true;
    }

    public static bool RefreshProceduralRotation(Transform tail) {
        var procRotation = tail.GetComponent<ProceduralRotation>();
        if (procRotation == null) {
            SceneEvents.InternalError("Not a Rotator", tail);
            return false;
        }

        procRotation.RefreshSaved();
        UndoHistory.current.Commit();
        SceneEvents.Refresh(Procedural.FirstProceduralChild(tail, true));
        Metrics.current.AddAction(PAction.RotationRefresh);
        return true;
    }

    public static bool RefreshAllLinksProceduralRotation(Transform tail) {
        var procRotation = tail.GetComponent<ProceduralRotation>();
        if (procRotation == null) {
            SceneEvents.InternalError("Not a Rotator", tail);
            return false;
        }

        foreach (Transform link in LinkedProcedural.GetLinks(tail)) {
            var proc = link.GetComponent<ProceduralRotation>();
            proc.RefreshSaved();
        }
        UndoHistory.current.Commit();
        SceneEvents.Refresh(Procedural.FirstProceduralChild(tail, true));
        Metrics.current.AddAction(PAction.RotationRefresh);
        return true;
    }

    public static bool AddAllLinksTilingChild(Transform tail) {
        var procTiling = tail.GetComponent<ProceduralTiling>();
        if (procTiling == null) {
            SceneEvents.InternalError("Not a Tiling", tail);
            return false;
        }

        procTiling.AddChildAllLinks();
        UndoHistory.current.Commit();
        SceneEvents.AddChild(tail);
        Metrics.current.AddAction(PAction.TilingAdd);
        Metrics.current.UpdateLargest(tail);
        return true;
    }

    public static bool RemoveAllLinksTilingChild(Transform tail) {
        var procTiling = tail.GetComponent<ProceduralTiling>();
        if (procTiling == null) {
            SceneEvents.InternalError("Not a Tiling", tail);
            return false;
        }
        if (Procedural.CountProceduralChildren(tail, true) == 1) {
            var child = Procedural.FirstProceduralChild(tail, true);
            SceneEvents.Error("Can't delete the last element", child);
            return false;
        }

        procTiling.RemoveChildAllLinks();
        UndoHistory.current.Commit();
        SceneEvents.RemoveChild(tail);
        Metrics.current.AddAction(PAction.TilingRemove);
        return true;
    }

    // ***** Individual operations in Grab mode or others

    public static bool TryDeleteTarget(Transform target) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("Can't delete (static)", target);
            return false;
        }

        SceneEvents.Delete(target);
        ProceduralFactory.Delete(target);
        UndoHistory.current.Commit();
        Metrics.current.AddAction(PAction.Delete);
        return true;
    }

    public static bool TryRandomizeTarget(Transform target, bool nestedOp = false) {
        var proc = target.GetComponent<Procedural>();
        if (!nestedOp && !Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("Can't modify (static)", target);
            return false;
        }
        if (proc.proctype.HasFlag(ProceduralType.Singular)) {
            SceneEvents.Info("Not a rule", target);
            return false;
        }
        if (!proc.modifiable) {
            SceneEvents.Info("Can't modify", target);
            return false;
        }

        SceneEvents.Randomize(target);
        ProceduralFactory.Randomize(target);
        UndoHistory.current.Commit();
        Metrics.current.AddAction(PAction.Randomize);
        return true;
    }

    public static bool TryUnlinkTarget(Transform target) {
        var proc = target.GetComponent<LinkedProcedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (proc == null) {
            SceneEvents.Info("No links", target);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("Can't unlink (static)", target);
            return false;
        }
        if (proc.proctype.HasFlag(ProceduralType.Singular)) {
            SceneEvents.Info("Not a rule", target);
            return false;
        }
        if (!LinkedProcedural.Unlinkable(target)) {
            SceneEvents.Info("No links", target);
            return false;
        }

        SceneEvents.Unlink(target);
        ProceduralFactory.Unlink(target);
        UndoHistory.current.Commit();
        Metrics.current.AddAction(PAction.Unlink);
        return true;
    }

    public static bool TryDisbandTarget(Transform target) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("Can't disband (static)", target);
            return false;
        }
        if (!proc.modifiable) {
            SceneEvents.Info("Can't disband", target);
            return false;
        }
        if (proc.proctype.HasFlag(ProceduralType.Singular)) {
            SceneEvents.Info("Not a rule", target);
            return false;
        }

        SceneEvents.Disband(target, proc.ProceduralChildren(true));
        ProceduralFactory.Disband(target);
        UndoHistory.current.Commit();
        Metrics.current.AddAction(PAction.Disband);
        return true;
    }

    // ***** Group Selection

    public static bool ToggleGroupSelection(DelegateLinkedSet<Transform> selection, Transform target) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("This is static; clone it first", target);
            return false;
        }

        if (selection.Toggle(target)) {
            SceneEvents.AddSelection(target);
        } else {
            SceneEvents.RemoveSelection(target);
        }
        return true;
    }

    public static bool CreateGroupSelection(DelegateLinkedSet<Transform> selection) {
        if (selection.Count <= 1) {
            SceneEvents.Info(string.Format("Only {0} elements selected", selection.Count));
            return false;
        }

        var group = ProceduralFactory.CreateGroup(new List<Transform>(selection));
        UndoHistory.current.Commit();
        selection.Clear();
        SceneEvents.CreateGroup(group.transform);
        Metrics.current.AddCreate(group.transform);
        return true;
    }

    // ***** Random Selection

    public static bool ToggleRandomSelection(DelegateLinkedSet<Transform> selection, Transform target) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("This is static; clone it first", target);
            return false;
        }

        if (selection.Toggle(target)) {
            SceneEvents.AddSelection(target);
        } else {
            SceneEvents.RemoveSelection(target);
        }
        return true;
    }

    public static bool CreateRandomSelection(DelegateLinkedSet<Transform> selection) {
        if (selection.Count <= 1) {
            SceneEvents.Info(string.Format("Only {0} variants selected", selection.Count));
            return false;
        }

        var random = ProceduralFactory.CreateRandom(new List<Transform>(selection));
        UndoHistory.current.Commit();
        selection.Clear();
        SceneEvents.CreateRandom(random.transform);
        Metrics.current.AddCreate(random.transform);
        return true;
    }

    // ***** Click based creators

    public static bool TryCreateRandomPosition(Transform target) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("This is static; clone it first", target);
            return false;
        }
        if (proc is ProceduralPosition) {
            SceneEvents.Info("Already a Mover; not allowed", target);
            return false;
        }

        var position = ProceduralFactory.CreatePosition(target);
        UndoHistory.current.Commit();
        SceneEvents.CreatePosition(position.transform);
        Metrics.current.AddCreate(position.transform);
        return true;
    }

    public static bool TryCreateRandomRotation(Transform target) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("This is static; clone it first", target);
            return false;
        }
        if (proc is ProceduralRotation) {
            SceneEvents.Info("Already a Rotator; not allowed", target);
            return false;
        }

        var rotation = ProceduralFactory.CreateRotation(target);
        UndoHistory.current.Commit();
        SceneEvents.CreateRotation(rotation.transform);
        Metrics.current.AddCreate(rotation.transform);
        return true;
    }

    public static bool TryCreateTiling(Transform target) {
        var proc = target.GetComponent<Procedural>();
        if (!Procedural.IsTopLevelProcedural(target)) {
            SceneEvents.InternalError("Not top level procedural", proc.transform);
            return false;
        }
        if (!proc.groupable) {
            SceneEvents.Info("This is static; clone it first", target);
            return false;
        }

        var tiling = ProceduralFactory.CreateTiling(target);
        UndoHistory.current.Commit();
        SceneEvents.CreateTiling(tiling.transform);
        Metrics.current.AddCreate(tiling.transform);
        return true;
    }

}
