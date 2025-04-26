/*using System.Collections.Generic;
using HunkHud.Modules;
using RoR2;
using UnityEngine;

namespace HunkHud.Components
{
    public class CameraAdjuster : MonoBehaviour
    {
        public float smoothSpeed = 16f;

        public float zoomSpeed = 0.5f;

        public bool allowZoom = true;

        private float checkStopwatch;

        private CharacterBody body;

        private Transform baseTransform;

        private Transform fakeBaseTransform;

        public Transform cameraTracker;

        private ChildLocator childLocator;

        private SphereSearch search;

        private CameraTargetParams camTargetParams;

        private CharacterCameraParams camParams;

        private List<HurtBox> hits = new List<HurtBox>();

        private float zoom;

        private float desiredZoom;

        public Vector3 maxZoom;

        public Vector3 minZoom;

        public float yOffset;

        private float desiredYOffset;

        public float baseYOffset = 0.04f;

        public float maxYOffset = 0.15f;

        public float maxDist = 8f;

        private bool isRor2Cam;
        private string cachedName;

        private void Awake()
        {
            this.body = GetComponent<CharacterBody>();
            var modelLoc = GetComponent<ModelLocator>();
            if (modelLoc && modelLoc.modelTransform)
            {
                this.childLocator = modelLoc.modelTransform.GetComponent<ChildLocator>();
                if (this.childLocator)
                {
                    this.baseTransform = transform;
                    this.baseTransform = this.childLocator.FindChild("Base");
                    if (!this.baseTransform)
                        this.baseTransform = this.childLocator.FindChild("Chest");

                    if (!this.baseTransform && (bool)this.body)
                        this.baseTransform = this.body.coreTransform;

                    this.fakeBaseTransform = this.childLocator.FindChild("FakeBase");
                    if (!this.fakeBaseTransform)
                    {
                        this.fakeBaseTransform = new GameObject("FakeBase").transform;
                        this.fakeBaseTransform.parent = modelLoc.modelTransform;
                        this.fakeBaseTransform.position = this.body ? this.body.corePosition : this.transform.position;
                    }

                    this.cameraTracker = this.childLocator.FindChild("CameraTracker");
                    if (!this.cameraTracker)
                    {
                        this.cameraTracker = new GameObject("CameraTracker").transform;
                        if (this.body)
                            this.cameraTracker.position = this.body.corePosition;
                    }

                    this.cameraTracker.transform.parent = null;
                    this.camTargetParams = GetComponent<CameraTargetParams>();
                    if (this.camTargetParams)
                        this.camParams = this.camTargetParams.cameraParams;
                }
            }

            this.desiredZoom = 0f;
            this.zoom = 0f;
            this.yOffset = 0f;
            this.desiredYOffset = 0f;

            this.isRor2Cam = !PluginConfig.overTheShoulderCamera.Value;
            if (!this.hunk && !PluginConfig.overTheShoulderCameraGlobal.Value)
            {
                DestroyImmediate(this);
                return;
            }

            this.search = new SphereSearch
            {
                mask = LayerIndex.entityPrecise.mask,
                radius = 60f
            };

            this.smoothSpeed = PluginConfig.cameraSmoothSpeed.Value;
            this.checkStopwatch = 0.5f;
            //if (this.isRor2Cam && (bool)this.hunk)
            //    this.smoothSpeed *= 1.5f;
            InvokeRepeating("SetParams", 0.1f, 5f);
        }

        private void SetParams()
        {
            var name = this.gameObject.name;
            if (string.Compare(name, this.cachedName) == 0)
                return;

            this.cachedName = name;
            this.minZoom = new Vector3(0.75f, 0.5f, -3.2f);
            this.maxZoom = new Vector3(1.2f, 1.5f, -10f);

            if (name.StartsWith("RobPaladin"))
                this.minZoom = new Vector3(1.2f, 1.5f, -6f);

            if (name.StartsWith("FalseSon"))
                this.minZoom = new Vector3(1.2f, 1.5f, -6f);

            if (name.StartsWith("Ravager"))
                this.minZoom = new Vector3(1f, 1.5f, -5f);

            if (name.StartsWith("HAND"))
                this.minZoom = new Vector3(0.75f, 2f, -7f);

            if (name.StartsWith("Toolbot"))
                this.minZoom = new Vector3(1f, 1.5f, -5f);

            if (name.StartsWith("Croco"))
                this.minZoom = new Vector3(1f, 1.5f, -5f);

            if (name.StartsWith("Enforcer"))
                this.minZoom = new Vector3(1f, 1.5f, -5f);

            if (name.StartsWith("Nemforcer"))
                this.minZoom = new Vector3(1f, 1.5f, -5f);

            if (name.StartsWith("Aliem"))
                this.minZoom = new Vector3(0.5f, 0.75f, -2.4f);

            if (name.StartsWith("RobNemesis"))
            {
                this.minZoom = new Vector3(2f, 3.5f, -10f);
                this.maxZoom = new Vector3(3.5f, 5f, -24f);
            }

            if (name.StartsWith("Regigigas"))
            {
                this.minZoom = new Vector3(3f, 10f, -20f);
                this.maxZoom = new Vector3(3.5f, 12f, -24f);
            }

            if (name.StartsWith("Tyranitar"))
            {
                this.minZoom = new Vector3(3f, 10f, -20f);
                this.maxZoom = new Vector3(3.5f, 12f, -24f);
            }

            if (name.StartsWith("Mechorilla"))
            {
                this.minZoom = new Vector3(3f, 10f, -20f);
                this.maxZoom = new Vector3(3.5f, 12f, -24f);
            }

            if (name.StartsWith("MegaDrone"))
            {
                this.minZoom = new Vector3(4f, 0.5f, -20f);
                this.maxZoom = new Vector3(4.5f, 0.5f, -30f);
            }

            if (name.StartsWith("RobHunkHelicopterBody"))
            {
                this.minZoom = new Vector3(7f, 2.5f, -16f);
                this.maxZoom = new Vector3(9f, 2.5f, -30f);
            }
        }

        private void Start()
        {
            if (this.body)
            {
                var camParams = this.body.GetComponent<CameraTargetParams>();
                if (camParams)
                    camParams.cameraPivotTransform = this.cameraTracker;
            }
        }

        private void OnDisable()
        {
            if (this.body && this.body.isPlayerControlled && this.body.hasAuthority && this.camParams && !this.isRor2Cam)
                this.camParams.data.idealLocalCameraPos = this.minZoom;
        }

        private void OnDestroy()
        {
            if (this.cameraTracker)
                Destroy(this.cameraTracker.gameObject);

            if (this.fakeBaseTransform)
                Destroy(this.fakeBaseTransform.gameObject);
        }

        private void Update()
        {
            this.checkStopwatch -= Time.deltaTime;
            if (this.checkStopwatch <= 0f && !this.isRor2Cam)
                this.CheckForEnemies();

            if (this.allowZoom && this.body)
                this.HandleZoom();
        }

        private void CheckForEnemies()
        {
            this.hits.Clear();
            this.search.ClearCandidates();
            this.search.origin = this.body.corePosition;
            this.search.RefreshCandidates();
            this.search.FilterCandidatesByDistinctHurtBoxEntities();
            this.search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(this.body.teamComponent.teamIndex));
            this.search.GetHurtBoxes(this.hits);
            var num = 0f;

            foreach (var hit in this.hits)
            {
                var body = hit.healthComponent.body;
                if (body)
                {
                    switch (body.hullClassification)
                    {
                        case HullClassification.Human:
                            num = Mathf.Max(num, 0.15f);
                            break;
                        case HullClassification.Golem:
                            num = Mathf.Max(num, 0.35f);
                            break;
                        case HullClassification.BeetleQueen:
                            num = Mathf.Max(num, 1f);
                            break;
                    };
                }
            }

            this.checkStopwatch = this.zoom < num ? 1f : 0.25f;
            this.desiredZoom = num;
        }

        private void HandleZoom()
        {
            if (!this.body || !this.body.isPlayerControlled || !this.body.hasAuthority || !this.camParams || this.isRor2Cam)
                return;
            if ((bool)this.hunk && this.hunk.counterTimer > 0f)
                this.desiredZoom = 0f;
            if ((bool)this.hunk && (bool)this.hunk.targetHurtbox && this.hunk.isRolling)
                this.desiredYOffset = this.baseYOffset;
            else
            {
                this.desiredYOffset = this.baseYOffset;
            }
            this.yOffset = Mathf.Lerp(this.yOffset, this.desiredYOffset, 6f * Time.deltaTime);
            if ((bool)this.nemesis && this.nemesis.isChanneling)
                this.desiredZoom = 0f;
            this.zoom = Mathf.Lerp(this.zoom, this.desiredZoom, this.zoomSpeed * Time.deltaTime);
            this.camParams.data.idealLocalCameraPos = Vector3.Lerp(this.minZoom, this.maxZoom, this.zoom);
            if (this.fakeBaseTransform)
                this.fakeBaseTransform.localPosition = new Vector3(0f, this.yOffset, 0f);
            if ((bool)this.hunk || !this.camTargetParams || this.camTargetParams.cameraParamsOverrides.Count <= 0)
                return;
            foreach (var cameraParamsOverride in this.camTargetParams.cameraParamsOverrides)
            {
                cameraParamsOverride.cameraParamsData.idealLocalCameraPos.value.y = this.camParams.data.idealLocalCameraPos.value.y;
                cameraParamsOverride.cameraParamsData.pivotVerticalOffset = this.camParams.data.pivotVerticalOffset;
            }
        }

        private void LateUpdate()
        {
            if (!this.cameraTracker || !this.baseTransform || !this.body || !this.fakeBaseTransform)
                return;
            if (this.smoothSpeed <= 0f)
            {
                if (this.cameraTracker)
                    this.cameraTracker.position = transform.position + new Vector3(0f, 0.6f, 0f);
                return;
            }
            var t = 0.75f;
            if ((bool)this.hunk && this.hunk.isRolling)
                t = 1f;
            if ((bool)this.hunk && this.hunk.isRolling && (bool)this.hunk.targetHurtbox)
                t = 0.5f;
            if (!this.hunk)
                t = 0.35f;
            var vector = Vector3.Lerp(this.fakeBaseTransform.position, this.baseTransform.position, t);
            var num = this.body.moveSpeed / this.body.baseMoveSpeed;
            if (num <= 0f)
                num = 1f;
            var num2 = this.smoothSpeed * num;
            if ((bool)this.hunk && (this.hunk.isRolling || this.hunk.immobilized))
                num2 = 80f;
            if ((bool)this.cameraTracker && (bool)this.baseTransform && (bool)this.body.inputBank)
                this.cameraTracker.position = Vector3.Lerp(this.cameraTracker.position, vector - this.body.inputBank.aimDirection, num2 * Time.deltaTime);
        }
    }
}
*/