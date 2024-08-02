// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    TheLacus
// 
// Notes:
//

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Utility.AssetInjection;
using Newtonsoft.Json;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// The component that handles graphics for a mobile person.
    /// </summary>
    /// <remarks>
    /// Implementations should be added to a prefab named "MobilePersonAsset" bundled with a mod.
    /// This prefab acts as a provider for all custom npcs graphics. This allows mods to replace classic
    /// <see cref="MobilePersonBillboard"/> with 3d models or other alternatives.
    /// The actual graphic asset for specific npc should be loaded when requested by <see cref="SetPerson"/>.
    /// </remarks>
    public abstract class MobilePersonAsset : MonoBehaviour
    {
        /// <summary>
        /// Trigger collider used for interaction with player. The collider should be altered to enclose the entire npc mesh when
        /// <see cref="SetPerson"/> is called. A sign of badly setup collider is misbehaviour of idle state and talk functionalities.
        /// </summary>
        protected internal CapsuleCollider Trigger { get; internal set; }

        /// <summary>
        /// Gets or sets idle state. Daggerfall NPCs are either in or motion or idle facing player.
        /// This only controls animation state, actual motion is handled by <see cref="MobilePersonMotor"/>.
        /// </summary>
        public abstract bool IsIdle { get; set; }

        /// <summary>
        /// Setup this person based on race, gender and outfit variant. Enitities in a npcs pool can be recycled
        /// when out of range, meaning that this method can be called more than once with different parameters.
        /// </summary>
        /// <param name="race">Race of target npc.</param>
        /// <param name="gender">Gender of target npc.</param>
        /// <param name="personVariant">Which basic outfit does the person wear.</param>
        /// <param name="isGuard">True if this npc is a city watch guard.</param>
        public abstract void SetPerson(Races race, Genders gender, int personVariant, bool isGuard, int personFaceVariant, int personFaceRecordId);

        /// <summary>
        /// Gets size of asset used by this person (i.e size of bounds). Used to adjust position on terrain.
        /// </summary>
        /// <returns>Size of npc.</returns>
        public abstract Vector3 GetSize();

        /// <summary>
        /// Gets a bitmask that provides all the layers used by this asset.
        /// </summary>
        /// <returns>A layer mask.</returns>
        public virtual int GetLayerMask()
        {
            return 1 << gameObject.layer;
        }
    }

    /// <summary>
    /// Billboard class for classic wandering NPCs found in town environments.
    /// </summary>
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MobilePersonBillboard : MobilePersonAsset
    {
        #region Fields

        const int numberOrientations = 8;
        const float anglePerOrientation = 360f / numberOrientations;

        Vector3 cameraPosition;
        Camera mainCamera = null;
        MeshFilter meshFilter = null;
        MeshRenderer meshRenderer = null;
        float facingAngle;
        int lastOrientation;
        AnimStates currentAnimState;

        Vector2[] recordSizes;
        int[] recordFrames;
        Rect[] atlasRects;
        RecordIndex[] atlasIndices;
        MobileAnimation[] stateAnims;
        MobileAnimation[] moveAnims;
        MobileAnimation[] idleAnims;
        MobileBillboardImportedTextures importedTextures;
        int currentFrame = 0;

        float animSpeed;
        float animTimer = 0;

        bool isUsingGuardTexture = false;

        #endregion

        #region Textures

        int[] maleBretonTextures = new int[] { 0100101, 0100102, 0100103, 0100104, 0100105, 0100106, 0100107, 0100108, 0100109, 0100110, 0100111, 0100112, 0100113, 0100114, 0100115, 0100116, 0100117, 0100118, 0100119, 0100120, 0100121, 0100122, 0100123,
                                               0100301, 0100302, 0100303, 0100304, 0100305, 0100306, 0100307, 0100308, 0100309, 0100310, 0100311, 0100312, 0100313, 0100314, 0100315, 0100316, 0100317, 0100318, 0100319, 0100320, 0100321, 0100322, 0100323, 0100324, 0100325,
                                               0100401, 0100402, 0100403, 0100404, 0100405, 0100406, 0100407, 0100408, 0100409, 0100410, 0100411, 0100412, 0100413, 0100414, 0100415, 0100416, 0100417, 0100418, 0100419, 0100420, 0100421, 0100422, 0100423, 0100424,
                                               0100601, 0100602, 0100603, 0100604, 0100605, 0100606, 0100607, 0100608, 0100609, 0100610, 0100611, 0100612, 0100613, 0100614, 0100615, 0100616, 0100617, 0100618, 0100619, 0100620, 0100621, 0100622, 0100623, 0100624 };    // { 385, 386, 391, 394 }
        int[] maleBretonDesertTextures = new int[] {};
        int[] maleBretonMountainTextures = new int[] { 0120401, 0120402, 0120403, 0120404, 0120405, 0120406, 0120407, 0120408, 0120409, 0120410, 0120411, 0120412, 0120413, 0120414, 0120415, 0120416, 0120417, 0120418, 0120419, 0120420, 0120421, 0120422, 0120423, 0120424 };
        int[] maleBretonColdTextures = new int[] { 0130101, 0130102, 0130103, 0130104, 0130105, 0130106, 0130107, 0130108, 0130109, 0130110, 0130111, 0130112, 0130113, 0130114, 0130115, 0130116, 0130117, 0130118, 0130119, 0130120, 0130121, 0130122, 0130123, 0130124 };

        int[] femaleBretonTextures = new int[] { 5100001, 5100002, 5100003, 5100004, 5100005, 5100006, 5100007, 5100008, 5100009, 5100010, 5100011, 5100012, 5100013, 5100014, 5100015,
                                                 5100101, 5100102, 5100103, 5100104, 5100105, 5100106, 5100107,
                                                 5100201, 5100202,
                                                 5100301, 5100302, 5100303, 5100304, 5100305, 5100306, 5100307, 5100308, 5100309, 5100310, 5100311, 5100312, 5100313, 5100314, 5100315,
                                                 5100401, 5100402, 5100403, 5100404, 5100405, 5100406, 5100407,
                                                 5100501, 5100502 };  // { 453, 454, 455, 456 }
        int[] femaleBretonDesertTextures = new int[] {};
        int[] femaleBretonMountainTextures = new int[] { 5120001, 5120002, 5120003, 5120004, 5120005, 5120006, 5120007, 5120008, 5120009, 5120010, 5120011, 5120012, 5120013, 5120014, 5120015, 5120016, 5120017, 5120018,
                                                         5120101, 5120102, 5120103, 5120104, 5120105, 5120106 };
        int[] femaleBretonColdTextures = new int[] { 5130001, 5130002, 5130003, 5130004, 5130005, 5130006, 5130007, 5130008, 5130009, 5130010, 5130011, 5130012, 5130013, 5130014, 5130015, 5130016, 5130017, 5130018, 5130019, 5130020, 5130021, 5130022, 5130023, 5130024 };

        int[] maleRedguardTextures = new int[] { 0200001, 0200002, 0200003, 0200004, 0200005, 0200006, 0200007, 0200008, 0200009, 0200010, 0200011, 0200012, 0200013, 0200014, 0200015, 0200016, 0200017, 0200018, 0200019, 0200020, 0200021, 0200022, 0200023, 0200024, 0200025, 0200026,
                                                 0200301, 0200302, 0200303, 0200304, 0200305, 0200306, 0200307, 0200308, 0200309, 0200310, 0200311, 0200312, 0200313, 0200314, 0200315, 0200316, 0200317, 0200318, 0200319, 0200320, 0200321, 0200322,
                                                 0200401, 0200402, 0200403, 0200404, 0200405, 0200406, 0200407, 0200408, 0200409, 0200410, 0200411, 0200412, 0200413, 0200414, 0200415, 0200416, 0200417, 0200418, 0200419, 0200420, 0200421, 0200422, 0200423, 0200424,
                                                 0200601, 0200602, 0200603, 0200604, 0200605, 0200606, 0200607, 0200608, 0200609, 0200610, 0200611, 0200612, 0200613, 0200614, 0200615, 0200616, 0200617, 0200618, 0200619, 0200620, 0200621, 0200622, 0200623, 0200624 };
        int[] maleRedguardDesertTextures = new int[] { 0210001, 0210002, 0210003, 0210004, 0210005, 0210006, 0210007, 0210008, 0210009, 0210010, 0210011, 0210012, 0210013, 0210014, 0210015, 0210016, 0210017, 0210018, 0210019, 0210020, 0210021, 0210022, 0210023, 0210024, 0210025, 0210026,
                                                       0210101, 0210102, 0210103, 0210104, 0210105, 0210106, 0210107, 0210108, 0210109, 0210110, 0210111, 0210112, 0210113, 0210114, 0210115, 0210116, 0210117, 0210118, 0210119, 0210120, 0210121, 0210122,
                                                       0210812, 0210816,
                                                       0210901, 0210902, 0210903, 0210904, 0210905, 0210906, 0210907, 0210908, 0210909, 0210910, 0210911, 0210912, 0210913, 0210914, 0210915, 0210916, 0210917, 0210918, 0210919, 0210920, 0210921, 0210922 };    // { 381, 382, 383, 384 }
        int[] maleRedguardMountainTextures = new int[] { 0220101, 0220102, 0220103, 0220104, 0220105, 0220106, 0220107, 0220108, 0220109, 0220110, 0220111, 0220112, 0220113, 0220114, 0220115, 0220116, 0220117, 0220118, 0220119, 0220120, 0220121, 0220122, 0220123, 0220124,
                                                         0220201, 0220202, 0220203, 0220204, 0220205, 0220206, 0220207, 0220208, 0220209, 0220210, 0220211, 0220212, 0220213, 0220214, 0220215, 0220216, 0220217, 0220218, 0220219, 0220220, 0220221, 0220222, 0220223, 0220224,
                                                         0220401, 0220402, 0220403, 0220404, 0220405, 0220406, 0220407, 0220408, 0220409, 0220410, 0220411, 0220412, 0220413, 0220414, 0220415, 0220416, 0220417, 0220418, 0220419, 0220420, 0220421, 0220422, 0220423, 0220424, 0220425, 0220426,
                                                         0220501, 0220502, 0220503, 0220504, 0220505, 0220506, 0220507, 0220508, 0220509, 0220510, 0220511, 0220512, 0220513, 0220514, 0220515, 0220516, 0220517, 0220518, 0220519, 0220520, 0220521, 0220522, 0220523, 0220524 };
        int[] maleRedguardColdTextures = new int[] { 0230400, 0230420, 0230426,
                                                     0230501, 0230502, 0230503, 0230504, 0230505, 0230506, 0230507, 0230508, 0230509, 0230510, 0230511, 0230512, 0230513, 0230514, 0230515, 0230516, 0230517, 0230518, 0230519, 0230520, 0230521, 0230522 };
        int[] femaleRedguardTextures = new int[] { 5200001, 5200002, 5200003, 5200004, 5200005, 5200006, 5200007, 5200008, 5200009, 5200010, 5200011, 5200012, 5200013, 5200014, 5200015, 5200016, 5200017, 5200018, 5200019, 5200020, 5200021, 5200022, 5200023, 5200024,
                                                   5200101, 5200102, 5200103, 5200104, 5200105, 5200106, 5200107, 5200108, 5200109, 5200110, 5200111, 5200112, 5200113, 5200114, 5200115, 5200116, 5200117, 5200118, 5200119, 5200120, 5200121, 5200122, 5200123, 5200124,
                                                   5200201, 5200202, 5200203, 5200204, 5200205, 5200206, 5200207, 5200208, 5200209, 5200210, 5200211, 5200212, 5200213, 5200214, 5200215, 5200216, 5200217, 5200218, 5200219, 5200220, 5200221, 5200222, 5200223, 5200224,
                                                   5200301, 5200302, 5200303, 5200304, 5200305, 5200306, 5200307, 5200308, 5200309, 5200310, 5200311, 5200312, 5200313, 5200314, 5200315, 5200316, 5200317, 5200318, 5200319, 5200320, 5200321, 5200322, 5200323, 5200324 };   // { 395, 396, 397, 398 }
        int[] femaleRedguardDesertTextures = new int[] { 5210001, 5210002, 5210003, 5210004, 5210005, 5210006, 5210007, 5210008, 5210009, 5210010, 5210011, 5210012, 5210013, 5210014, 5210015, 5210016, 5210017, 5210018, 5210019, 5210020, 5210021, 5210022, 5210023, 5210024,
                                                         5210101, 5210102, 5210103, 5210104, 5210105, 5210106, 5210107, 5210108, 5210109, 5210110, 5210111, 5210112, 5210113, 5210114, 5210115, 5210116, 5210117, 5210118, 5210119, 5210120, 5210121, 5210122, 5210123, 5210124 } ;  // { 395, 396, 397, 398 }
        int[] femaleRedguardMountainTextures = new int[] { 5220001, 5220002, 5220003, 5220004, 5220005, 5220006, 5220007, 5220008, 5220009, 5220010, 5220011, 5220012, 5220013, 5220014, 5220015, 5220016, 5220017, 5220018, 5220019, 5220020, 5220021, 5220022, 5220023, 5220024 };
        int[] femaleRedguardColdTextures = new int[] { 5230101, 5230102, 5230103, 5230104, 5230105, 5230106, 5230107, 5230108, 5230109, 5230110, 5230111, 5230112, 5230113, 5230114, 5230115, 5230116, 5230117, 5230118, 5230119, 5230120, 5230121, 5230122, 5230123, 5230124,
                                                       5230301, 5230302, 5230303, 5230304, 5230305, 5230306, 5230307, 5230308, 5230309, 5230310, 5230311, 5230312, 5230313, 5230314, 5230315, 5230316, 5230317, 5230318, 5230319, 5230320, 5230321, 5230322, 5230323, 5230324 };

        int[] maleNordTextures = new int[] { 0300801, 0300802, 0300803, 0300804, 0300805, 0300806, 0300807, 0300808, 0300809, 0300810, 0300811, 0300812, 0300813, 0300814, 0300815, 0300816, 0300817, 0300818, 0300819, 0300820, 0300821, 0300822, 0300823, 0300824 };  // { 387, 388, 389, 390 }
        int[] maleNordDesertTextures = new int[] {};
        int[] maleNordMountainTextures = new int[] { 0320101, 0320102, 0320103, 0320104, 0320105, 0320106, 0320107, 0320108, 0320109, 0320110, 0320111, 0320112, 0320113, 0320114, 0320115, 0320116, 0320117, 0320118, 0320119, 0320120, 0320121, 0320122, 0320123,
                                                     0320301, 0320302, 0320303, 0320304, 0320305, 0320306, 0320307, 0320308, 0320309, 0320310, 0320311, 0320312, 0320313, 0320314, 0320315, 0320316, 0320317, 0320318, 0320319, 0320320, 0320321, 0320322, 0320323, 0320324 };
        int[] maleNordColdTextures = new int[] { 0330401, 0330402, 0330403, 0330404, 0330405, 0330406, 0330407, 0330408, 0330409, 0330410, 0330411, 0330412, 0330413, 0330414, 0330415, 0330416, 0330417, 0330418, 0330419, 0330420, 0330421, 0330422, 0330423, 0330424,
                                                 0330501, 0330502, 0330503, 0330504, 0330505, 0330506, 0330507, 0330508, 0330509, 0330510, 0330511, 0330512, 0330513, 0330514, 0330515, 0330516, 0330517, 0330518, 0330519, 0330520, 0330521, 0330522, 0330523, 0330524,
                                                 0330701, 0330702, 0330703, 0330704, 0330705, 0330706, 0330707, 0330708, 0330709, 0330710, 0330711, 0330712, 0330713, 0330714, 0330715, 0330716, 0330717, 0330718, 0330719, 0330720, 0330721, 0330722, 0330723,
                                                 0330901, 0330902, 0330903, 0330904, 0330905, 0330906, 0330907, 0330908, 0330909, 0330910, 0330911, 0330912, 0330913, 0330914, 0330915, 0330916, 0330917, 0330918, 0330919, 0330920, 0330921, 0330922, 0330923, 0330924 };
        int[] femaleNordTextures = new int[] { 5300001, 5300002, 5300003, 5300004, 5300005, 5300006, 5300007, 5300008, 5300009, 5300010, 5300011, 5300012,
                                               5300101, 5300102,
                                               5300201, 5300202, 5300203, 5300204, 5300205, 5300206, 5300207 };    // { 392, 393, 451, 452 }
        int[] femaleNordDesertTextures = new int[] {};
        int[] femaleNordMountainTextures = new int[] { 5320001, 5320002, 5320003, 5320004, 5320005, 5320006, 5320007, 5320008, 5320009, 5320010, 5320011, 5320012, 5320013, 5320014, 5320015, 5320016, 5320017, 5320018, 5320019, 5320020, 5320021, 5320022, 5320023, 5320024,
                                                       5320101, 5320102, 5320103, 5320104, 5320105, 5320106, 5320107, 5320108, 5320109, 5320110, 5320111, 5320112, 5320113, 5320114, 5320115, 5320116, 5320117, 5320118, 5320119, 5320120, 5320121, 5320122, 5320123, 5320124 };
        int[] femaleNordColdTextures = new int[] { 5330001, 5330002, 5330003, 5330004, 5330005, 5330006, 5330007, 5330008, 5330009, 5330010, 5330011, 5330012, 5330013, 5330014, 5330015, 5330016, 5330017, 5330018, 5330019, 5330020, 5330021, 5330022, 5330023, 5330024 };

        int[] maleDarkElfTextures = new int[] { 0400001, 
                                                0400101, 0400102, 0400103 };
        int[] maleDarkElfDesertTextures = new int[] { 0410001,
                                                      0410101, 0410102, 0410103 };
        int[] maleDarkElfMountainTextures = new int[] {};
        int[] maleDarkElfColdTextures = new int[] {};
        int[] femaleDarkElfTextures = new int[] { 5400001,
                                                  5400101, 5400102 };
        int[] femaleDarkElfDesertTextures = new int[] { 5410001, 
                                                       5410101, 5410102, 5410103 };
        int[] femaleDarkElfMountainTextures = new int[] {};
        int[] femaleDarkElfColdTextures = new int[] {};

        int[] guardTextures = { 399, 3990017, 3990020, 3990023, 3990200, 3990016 };

        #endregion

        #region Animations

        const int IdleAnimSpeed = 1;
        const int MoveAnimSpeed = 4;
        static MobileAnimation[] IdleAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 5, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},          // Idle
        };

        static MobileAnimation[] IdleAnimsGuard = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 15, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},          // Guard idle
        };

        static MobileAnimation[] MoveAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 0, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},          // Facing south (front facing player)
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},          // Facing south-west
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},          // Facing west
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},          // Facing north-west
            new MobileAnimation() {Record = 4, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},          // Facing north (back facing player)
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},           // Facing north-east
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},           // Facing east
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},           // Facing south-east
        };

        enum AnimStates
        {
            Idle,           // Idle facing player
            Move,           // Moving
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets idle state.
        /// Daggerfall NPCs are either in or motion or idle facing player.
        /// This only controls anim state, actual motion is handled by MobilePersonMotor.
        /// </summary>
        public sealed override bool IsIdle
        {
            get { return (currentAnimState == AnimStates.Idle); }
            set { SetIdle(value); }
        }

        #endregion

        #region Unity

        private void Start()
        {
            if (Application.isPlaying)
            {
                // Get component references
                mainCamera = GameManager.Instance.MainCamera;
                meshFilter = GetComponent<MeshFilter>();
            }

            // Mobile NPC shadows if enabled
            if (DaggerfallUnity.Settings.MobileNPCShadows)
                GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        }

        private void Update()
        {
            // Rotate to face camera in game
            if (mainCamera && Application.isPlaying)
            {
                // Rotate billboard based on camera facing
                cameraPosition = mainCamera.transform.position;
                Vector3 viewDirection = -new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
                transform.LookAt(transform.position + viewDirection);

                // Orient based on camera position
                UpdateOrientation();

                // Tick animations
                animTimer += Time.deltaTime;
                if (animTimer > 1f / animSpeed)
                {
                    if (++currentFrame >= stateAnims[0].NumFrames)
                        currentFrame = 0;

                    UpdatePerson(lastOrientation);
                    animTimer = 0;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setup this person based on race and gender.
        /// </summary>
        public override void SetPerson(Races race, Genders gender, int personVariant, bool isGuard, int personFaceVariant, int personFaceRecordId)
        {
            // Must specify a race
            if (race == Races.None)
                return;

            // Get texture range for this race and gender
            int[] textures = null;
            int texture = 0;

            isUsingGuardTexture = isGuard;
            MapsFile.Climates climate = (MapsFile.Climates)GameManager.Instance.PlayerGPS.CurrentClimateIndex;
            DaggerfallDateTime.Seasons season = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ActualSeasonValue;

            if (isGuard)
            {
                textures = guardTextures;
            }
            else
            {
                textures = GetTextureNumber(race, climate, season, gender);
                personVariant = UnityEngine.Random.Range(0, textures.Length);
            }

            // Setup person rendering
            CacheRecordSizesAndFrames(textures[personVariant]);
            AssignMeshAndMaterial(textures[personVariant]);

            // Setup animation state
            moveAnims = GetStateAnims(AnimStates.Move);
            idleAnims = GetStateAnims(AnimStates.Idle);
            stateAnims = moveAnims;
            animSpeed = stateAnims[0].FramePerSecond;
            currentAnimState = AnimStates.Move;
            lastOrientation = -1;
            UpdateOrientation();
        }

        /// <summary>
        /// Get NPC billboard based on race, climate, season and gender.
        /// </summary>
        public int[] GetTextureNumber(Races race, MapsFile.Climates climate, DaggerfallDateTime.Seasons season, Genders gender)
        {
            int texture, resultRace, resultTemp;
            resultRace = (int)race;

            switch (climate)
            {
                case MapsFile.Climates.Desert:
                case MapsFile.Climates.Desert2:
                    resultTemp = 1;
                    break;

                case MapsFile.Climates.Mountain:
                case MapsFile.Climates.MountainWoods:
                    switch (season)
                    {
                        case DaggerfallDateTime.Seasons.Summer:
                            resultTemp = 0;
                            break;

                        case DaggerfallDateTime.Seasons.Spring:
                        case DaggerfallDateTime.Seasons.Fall:
                            resultTemp = 2;
                            break;

                        case DaggerfallDateTime.Seasons.Winter:
                        default:
                            resultTemp = 3;
                            break;
                    }
                    break;

                case MapsFile.Climates.Rainforest:
                case MapsFile.Climates.Subtropical:
                    switch (season)
                    {
                        case DaggerfallDateTime.Seasons.Spring:
                        case DaggerfallDateTime.Seasons.Summer:
                            resultTemp = 1;
                            break;

                        case DaggerfallDateTime.Seasons.Fall:
                        case DaggerfallDateTime.Seasons.Winter:
                        default:
                            resultTemp = 0;
                            break;
                    }
                    break;

                case MapsFile.Climates.Swamp:
                case MapsFile.Climates.Woodlands:
                case MapsFile.Climates.HauntedWoodlands:
                case MapsFile.Climates.Maquis:
                    switch (season)
                    {
                        case DaggerfallDateTime.Seasons.Spring:
                        case DaggerfallDateTime.Seasons.Summer:
                            resultTemp = 0;
                            break;

                        case DaggerfallDateTime.Seasons.Fall:
                            resultTemp = 2;
                            break;

                        case DaggerfallDateTime.Seasons.Winter:
                        default:
                            resultTemp = 3;
                            break;
                    }
                    break;
                
                default:
                    resultTemp = 0;
                    break;
            }

            texture = resultRace * 10 + resultTemp;
            if (gender == Genders.Female) texture += 500;

            int[] textures = GetTextureType(texture);

            if (textures.Length == 0)
                textures = GetTextureType(texture / 10 * 10);   // if there are no billboards for that particular climate/season, we'll just dish out a regular (i.e.: temperate) townfolk of that race.

            return textures;
        }

        public int[] GetTextureType(int texture)
        {
            switch (texture)
            {
                case 10: return maleBretonTextures;
                case 11: return maleBretonDesertTextures;
                case 12: return maleBretonMountainTextures;
                case 13: return maleBretonColdTextures;
                
                case 20: return maleRedguardTextures;
                case 21: return maleRedguardDesertTextures;
                case 22: return maleRedguardMountainTextures;
                case 23: return maleRedguardColdTextures;

                case 30: return maleNordTextures;
                case 31: return maleNordDesertTextures;
                case 32: return maleNordMountainTextures;
                case 33: return maleNordColdTextures;

                case 40: return maleDarkElfTextures;
                case 41: return maleDarkElfDesertTextures;
                case 42: return maleDarkElfMountainTextures;
                case 43: return maleDarkElfColdTextures;

                case 510: return femaleBretonTextures;
                case 511: return femaleBretonDesertTextures;
                case 512: return femaleBretonMountainTextures;
                case 513: return femaleBretonColdTextures;

                case 520: return femaleRedguardTextures;
                case 521: return femaleRedguardDesertTextures;
                case 522: return femaleRedguardMountainTextures;
                case 523: return femaleRedguardColdTextures;

                case 530: return femaleNordTextures;
                case 531: return femaleNordDesertTextures;
                case 532: return femaleNordMountainTextures;
                case 533: return femaleNordColdTextures;

                case 540: return femaleDarkElfTextures;
                case 541: return femaleDarkElfDesertTextures;
                case 542: return femaleDarkElfMountainTextures;
                case 543: return femaleDarkElfColdTextures;

                default: return maleBretonTextures;
            }
        }

        /// <summary>
        /// Gets billboard size.
        /// </summary>
        /// <returns>Vector2 of billboard width and height.</returns>
        public sealed override Vector3 GetSize()
        {
            if (recordSizes == null || recordSizes.Length == 0)
                return Vector2.zero;

            return recordSizes[0];
        }

        #endregion

        #region Private Methods

        void SetIdle(bool idle)
        {
            if (idle)
            {
                // Switch animation state to idle
                currentAnimState = AnimStates.Idle;
                stateAnims = idleAnims;
                currentFrame = 0;
                lastOrientation = -1;
                animTimer = 1;
                animSpeed = stateAnims[0].FramePerSecond;
            }
            else
            {
                // Switch animation state to move
                currentAnimState = AnimStates.Move;
                stateAnims = moveAnims;
                currentFrame = 0;
                lastOrientation = -1;
                animTimer = 1;
                animSpeed = stateAnims[0].FramePerSecond;
            }
        }

        private void CacheRecordSizesAndFrames(int textureArchive)
        {
            // Open texture file
            string path;
            if (textureArchive < 1000)
                path = Path.Combine(DaggerfallUnity.Instance.Arena2Path, TextureFile.IndexToFileName(textureArchive));
            // else path = Path.Combine(WorldMaps.mapPath, "Textures", TextureFile.IndexToFileName(textureArchive));
            else if (File.Exists(Path.Combine(WorldMaps.mapPath, "Textures", TextureFile.IndexToFileName(textureArchive))))
                path = Path.Combine(WorldMaps.mapPath, "Textures", TextureFile.IndexToFileName(textureArchive));
            else path = Path.Combine(WorldMaps.mapPath, "Textures", TextureFile.IndexToFileName(textureArchive).Substring(0, 13) + "00");
            Debug.Log("path: " + path);
            TextureFile textureFile = new TextureFile(path, FileUsage.UseMemory, true);
            Debug.Log("textureFile.RecordCount: " + textureFile.RecordCount);

            // Cache size and scale for each record
            recordSizes = new Vector2[textureFile.RecordCount];
            recordFrames = new int[textureFile.RecordCount];
            
            for (int i = 0; i < textureFile.RecordCount; i++)
            {
                // Get size and scale of this texture
                DFSize size = textureFile.GetSize(i);
                DFSize scale = textureFile.GetScale(i);

                // Set start size
                Vector2 startSize;
                startSize.x = size.Width;
                startSize.y = size.Height;

                // Apply scale
                Vector2 finalSize;
                int xChange = (int)(size.Width * (scale.Width / BlocksFile.ScaleDivisor));
                int yChange = (int)(size.Height * (scale.Height / BlocksFile.ScaleDivisor));
                finalSize.x = (size.Width + xChange);
                finalSize.y = (size.Height + yChange);

                // Set optional scale
                TextureReplacement.SetBillboardScale(textureArchive, i, ref finalSize);

                // Store final size and frame count
                recordSizes[i] = finalSize * MeshReader.GlobalScale;
                recordFrames[i] = textureFile.GetFrameCount(i);
            }
        }

        private void AssignMeshAndMaterial(int textureArchive)
        {
            // Get mesh filter
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            // Vertices for a 1x1 unit quad
            // This is scaled to correct size depending on facing and orientation
            float hx = 0.5f, hy = 0.5f;
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(hx, hy, 0);
            vertices[1] = new Vector3(-hx, hy, 0);
            vertices[2] = new Vector3(hx, -hy, 0);
            vertices[3] = new Vector3(-hx, -hy, 0);

            // Indices
            int[] indices = new int[6]
            {
                0, 1, 2,
                3, 2, 1,
            };

            // Normals
            Vector3 normal = Vector3.Normalize(Vector3.up + Vector3.forward);
            Vector3[] normals = new Vector3[4];
            normals[0] = normal;
            normals[1] = normal;
            normals[2] = normal;
            normals[3] = normal;

            // Create mesh
            Mesh mesh = new Mesh();
            mesh.name = string.Format("MobilePersonMesh");
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.normals = normals;

            // Assign mesh
            meshFilter.sharedMesh = mesh;

            // Create material
            Material material = TextureReplacement.GetMobileBillboardMaterial(textureArchive, meshFilter, ref importedTextures) ??
                DaggerfallUnity.Instance.MaterialReader.GetMaterialAtlas(
                textureArchive,
                0,
                4,
                1024,
                out atlasRects,
                out atlasIndices,
                4,
                true,
                0,
                false,
                true);

            // Set new person material
            GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void UpdateOrientation()
        {
            Transform parent = transform.parent;
            if (parent == null)
                return;

            // Get direction normal to camera, ignore y axis
            Vector3 dir = Vector3.Normalize(
                new Vector3(cameraPosition.x, 0, cameraPosition.z) -
                new Vector3(transform.position.x, 0, transform.position.z));

            // Get parent forward normal, ignore y axis
            Vector3 parentForward = transform.parent.forward;
            parentForward.y = 0;

            // Get angle and cross product for left/right angle
            facingAngle = Vector3.Angle(dir, parentForward);
            facingAngle = facingAngle * -Mathf.Sign(Vector3.Cross(dir, parentForward).y);

            // Facing index
            int orientation = - Mathf.RoundToInt(facingAngle / anglePerOrientation);
            // Wrap values to 0 .. numberOrientations-1
            orientation = (orientation + numberOrientations) % numberOrientations;

            // Change person to this orientation
            if (orientation != lastOrientation)
                UpdatePerson(orientation);
        }

        private void UpdatePerson(int orientation)
        {
            if (stateAnims == null || stateAnims.Length == 0)
                return;

            // Get mesh filter
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            // Idle only has a single orientation
            if (currentAnimState == AnimStates.Idle && orientation != 0)
                orientation = 0;

            // Get person size and scale for this state
            int record = stateAnims[orientation].Record;
            Vector2 size = recordSizes[record];

            // Set mesh scale for this state
            transform.localScale = new Vector3(size.x, size.y, 1);

            // Check if orientation flip needed
            bool flip = stateAnims[orientation].FlipLeftRight;

            // Update Record/Frame texture
            if (importedTextures.HasImportedTextures)
            {
                if (meshRenderer == null)
                    meshRenderer = GetComponent<MeshRenderer>();

                // Assign imported texture
                Debug.Log("record: " + record);
                meshRenderer.sharedMaterial.mainTexture = importedTextures.Albedo[record][currentFrame];
                if (importedTextures.IsEmissive)
                    meshRenderer.material.SetTexture(Uniforms.EmissionMap, importedTextures.EmissionMaps[record][currentFrame]);

                // Update UVs on mesh
                Vector2[] uvs = new Vector2[4];
                if (flip)
                {
                    uvs[0] = new Vector2(1, 1);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(1, 0);
                    uvs[3] = new Vector2(0, 0);
                }
                else
                {
                    uvs[0] = new Vector2(0, 1);
                    uvs[1] = new Vector2(1, 1);
                    uvs[2] = new Vector2(0, 0);
                    uvs[3] = new Vector2(1, 0);
                }
                meshFilter.sharedMesh.uv = uvs;
            }
            else
            {
                // Daggerfall Atlas: Update UVs on mesh
                Debug.Log("atlasRects.Length: " + atlasRects.Length + ", atlasIndices.Length: " + atlasIndices.Length + "record: " + record);
                Rect rect = atlasRects[atlasIndices[record].startIndex + currentFrame];
                Vector2[] uvs = new Vector2[4];
                if (flip)
                {
                    uvs[0] = new Vector2(rect.xMax, rect.yMax);
                    uvs[1] = new Vector2(rect.x, rect.yMax);
                    uvs[2] = new Vector2(rect.xMax, rect.y);
                    uvs[3] = new Vector2(rect.x, rect.y);
                }
                else
                {
                    uvs[0] = new Vector2(rect.x, rect.yMax);
                    uvs[1] = new Vector2(rect.xMax, rect.yMax);
                    uvs[2] = new Vector2(rect.x, rect.y);
                    uvs[3] = new Vector2(rect.xMax, rect.y);
                }
                meshFilter.sharedMesh.uv = uvs;
            }

            // Assign new orientation
            lastOrientation = orientation;
        }

        private MobileAnimation[] GetStateAnims(AnimStates state)
        {
            // Clone static animation state
            MobileAnimation[] anims;
            switch (state)
            {
                case AnimStates.Move:
                    anims = (MobileAnimation[])MoveAnims.Clone();
                    break;
                case AnimStates.Idle:
                    if (isUsingGuardTexture)
                        anims = (MobileAnimation[])IdleAnimsGuard.Clone();
                    else
                        anims = (MobileAnimation[])IdleAnims.Clone();
                    break;
                default:
                    return null;
            }

            // Assign number number of frames per anim
            Debug.Log("anims.Length: " + anims.Length + ", recordFrames.Length: " + recordFrames.Length);
            for (int i = 0; i < anims.Length; i++)
                anims[i].NumFrames = recordFrames[anims[i].Record];

            return anims;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        /// <summary>
        /// Rotate person to face editor camera while game not running.
        /// </summary>
        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                // Rotate to face camera
                UnityEditor.SceneView sceneView = GetActiveSceneView();
                if (sceneView)
                {
                    // Editor camera stands in for player camera in edit mode
                    cameraPosition = sceneView.camera.transform.position;
                    Vector3 viewDirection = -new Vector3(sceneView.camera.transform.forward.x, 0, sceneView.camera.transform.forward.z);
                    transform.LookAt(transform.position + viewDirection);
                    UpdateOrientation();
                }
            }
        }

        private SceneView GetActiveSceneView()
        {
            // Return the focused window if it is a SceneView
            if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof(SceneView))
                return (SceneView)EditorWindow.focusedWindow;

            return null;
        }
#endif

    #endregion
    }
}