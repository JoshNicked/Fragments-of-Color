// FRAGMENT PERSISTENCE SETUP GUIDE
// ===================================
// 
// This system allows fragments collected in one level to persist and be available in subsequent levels.
//
// SETUP STEPS:
// ============
//
// 1. CREATE FRAGMENT REGISTRY:
//    - In your scene (e.g., Level_1, Level_2, Level_3), create an empty GameObject
//    - Name it "FragmentRegistry"
//    - Attach the FragmentRegistry script to it
//    - In the Inspector, set the size of the "Fragments" array to match your fragments
//    - For each fragment, set:
//      * Fragment Name: (e.g., "blue_fragment", "red_fragment")
//      * Icon: (the Sprite used in hotbar UI)
//      * Prefab: (the 3D model/prefab to spawn in hand)
//    
//    NOTE: FragmentRegistry should be present in ALL levels (Level_1, Level_2, Level_3)
//    and should have identical fragment entries in all levels.
//
// 2. ENSURE HOTBAR SYSTEM IS CONFIGURED:
//    - HotbarSystem component should have:
//      * slotImages: array of Image UI elements for the 5 hotbar slots
//      * handPoint: Transform at player's hand where equipped items appear
//      * playerAnimator: Animator component of the player
//      * autoLoadFragments: ENABLED (checked in Inspector)
//
// 3. HOW IT WORKS:
//    - Level 1: Player opens chest, collects "blue_fragment"
//      * ChestInteraction calls hotbar.AddItem()
//      * HotbarSystem.AddItem() saves fragment name to PlayerPrefs
//      * Fragment appears in hotbar slot 1
//    
//    - Level 2: Player enters new scene
//      * HotbarSystem.Start() calls LoadHotbar()
//      * LoadHotbar() reads FragmentRegistry and rebuilds inventory
//      * "blue_fragment" appears in hotbar slot 1
//    
//    - Level 3: Process repeats
//
// 4. KEY METHODS:
//    - SaveHotbar(): Manually save current hotbar (called automatically by AddItem)
//    - LoadHotbar(): Manually load saved fragments (called automatically in Start)
//    - ClearSavedFragments(): Clear all saved fragments (useful for testing/new game)
//
// 5. TESTING:
//    - In Level_1, collect a fragment
//    - Press Play in Level_2 (or load Level_2 directly)
//    - The collected fragment should appear in hotbar slot 1
//    - Press 1 to equip it
//
// 6. TROUBLESHOOTING:
//    - If fragments don't appear: Check Fragment Registry is in scene and autoLoadFragments is enabled
//    - If wrong fragments appear: Verify fragment names match between registry and prefabs
//    - To reset: Call HotbarSystem.ClearSavedFragments() or delete PlayerPrefs keys manually
//
// FRAGMENT NAME MUST MATCH:
// ========================
// When collecting an item via ChestInteraction, the prefab name should match
// an entry in FragmentRegistry's fragments list.
//
// Example:
//   - FragmentRegistry has fragment "blue_fragment"
//   - ChestInteraction prefab gameObject is named "blue_fragment"
//   - Add at Chest: hotbar.AddItem(icon, prefab) where prefab.name == "blue_fragment"
//   - LoadHotbar() will find it via FragmentRegistry.GetFragment("blue_fragment")
