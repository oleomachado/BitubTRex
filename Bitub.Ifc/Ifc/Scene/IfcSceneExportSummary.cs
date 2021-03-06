﻿using Bitub.Dto.Scene;

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf.WellKnownTypes;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Scene
{
    public sealed class IfcSceneExportSummary
    {
        public readonly IModel Model;
        public readonly SceneModel Scene;
        public readonly IfcSceneExportSettings AppliedSettings;
        public readonly IDictionary<int, Component> ComponentCache;
        public readonly double Scale;

        #region Internals

        private readonly IDictionary<int, Tuple<SceneContext, XbimMatrix3D>> Context;

        internal IfcSceneExportSummary(IModel model, IfcSceneExportSettings settings)
        {
            Model = model;
            AppliedSettings = settings;
            Context = new SortedDictionary<int, Tuple<SceneContext, XbimMatrix3D>>();
            ComponentCache = new Dictionary<int, Component>();
            Scale = settings.UnitsPerMeter / model.ModelFactors.OneMeter;

            var p = model.Instances.OfType<IIfcProject>().First();
            Scene = CreateNew(p, settings);

            var instanceContexts = settings.UserRepresentationContext.Select(c => new SceneContext
            {
                Name = c.Name,
                // Given in DEG => use as it is
                FDeflection = p.Model.ModelFactors.DeflectionAngle,
                // Given internally in model units => convert to meter
                FTolerance = model.ModelFactors.LengthToMetresConversionFactor * p.Model.ModelFactors.DeflectionTolerance,
            }).ToArray();

            Scene.Contexts.AddRange(instanceContexts);
            settings.UserRepresentationContext = instanceContexts;
        }

        private SceneModel CreateNew(IIfcProject p, IfcSceneExportSettings settings)
        {
            return new SceneModel()
            {
                Name = p?.Name,
                Id = p?.GlobalId.ToGlobalUniqueId(),
                UnitsPerMeter = settings.UnitsPerMeter,
                Stamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
        }

        internal void MarkAsFailure(Exception cause)
        {
            Context.Clear();
            ComponentCache.Clear();
            Scene.Components.Clear();
            Scene.Materials.Clear();
            Scene.Contexts.Clear();
            FailureReason = cause;
        }

        #endregion

        public Exception FailureReason { get; internal set; }

        public int[] ExportedContextLabels => Context.Keys.ToArray();

        public Tuple<SceneContext, XbimMatrix3D> RepresentationContext(int contextLabel)
        {
            Tuple<SceneContext, XbimMatrix3D> sc;
            if (Context.TryGetValue(contextLabel, out sc))
                return sc;
            else
                return null;
        }

        internal void SetRepresentationContext(int contextLabel, SceneContext sc, XbimMatrix3D wcs)
        {
            Context[contextLabel] = new Tuple<SceneContext, XbimMatrix3D>(sc, wcs);
        }

        public SceneContext ContextOf(int contextLabel) => RepresentationContext(contextLabel)?.Item1;
        
        public XbimMatrix3D? TransformOf(int contextLabel) => RepresentationContext(contextLabel)?.Item2;

        public bool IsInContext(int contextLabel) => Context.ContainsKey(contextLabel);        
    }
}
