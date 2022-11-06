using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLAN : MonoBehaviour
{
    //          PLAN for SKIRMISH/BLOCK-GEN

    // ---------------------------------------------------------------------------------------------------------------------
    //              MAIN Features

    // Generates Pieces Using Prefabs (100%)
    // - Spawns a prefab (DONE)
    // - Can be Random and not using the prefabs (DONE)
    // - can be used between all Major Uses (DONE)

    // Generates Buildings Using Prefabs (DONE)
    // - Building spots need to be made (DONE)
    // - Need Buildings (DONE) 

    // Generates Props for The Level using prefabs (50%)
    // - Needs Prop Spots (DONE)
    // - props need to be made (0%)
    // - Props Break (50% - Needs Testing)

    // Generates Using a Grid System (90-0%)
    // - Grids can be edited (DONE)
    // - Needs to be one Script

    // Player/Enemy Base Does not Get Affected (15% BETA)
    // - Player Base and enemy Base
    // - player base Needs to spawn inside the Gen Without being edited 
    // - Buildings are Random Gen
    // - Building Spots are not affected

    // Generates a Navmesh (10% "BETA")
    // - Makes the nav mesh in start
    // - Assign Tags
    // - Can be done to all Prefabs 

    // Events can be spawn and played (20% BETA)
    //      LIST (needs testing/Work On)
    // - AMBUSH (50% - Needs Testing)
    // - Spawn Object (50% - Needs Testing)
    // - Remove Object (50% - Needs Testing)
    // - Timer Script (50% - Needs Improvements)
    // - Particle Play (20% Needs to be setup and assign to job)
    // - Landmine (Need Help with damage)
    // - Play Mission UI Logs
    // - Object Swap 
    // - Play Audio 
    // - Change Colour/Texture

    // Pathfinding Generation (0% "BETA")
    // - Makes a path to the Enemy Base / Bases
    // - Generates using te grid 
    // - Tracks location of player base to Prevent Border Issues

    // Piece Connect System (0% "BETA"
    // - all prefabs are assign to Number
    // - Able to generate using Connection
    // - Generates new if they can't connect 
    //      TYPE
    //0 Empty
    //1 Road
    //2 Building
    //3 Build Connect
    //4 Path 
    //5 Event
    //6
    //7
    //8
    //9


    // ---------------------------------------------------------------------------------------------------------------------
    //              SIDE CONTENT

    // Random Images for the splash art for UI (50% - spawn image type and not Double up need to swap)
    // Custom Map Size using one scene (WIP - Needs Corey GUI - BETA)
    // generate Outposts or Have option to spawn outpost (BETA)
    // Edit Player Base without effects (BETA)
    // Can Use its own rotation SYSTEM (GOLD)
    // Scripts be user Friendly (ALL Milestones)
    // Be Easy To USE for Designers (ALL Milestone)
    // use Debug.LOG for things 

    // ---------------------------------------------------------------------------------------------------------------------
    //              CHECKS FOR QA

    // when running spawns the object
    // spawns it in the empty game object
    // no issues occurred 
    // stays inside the game object
    // Scripts can play
    // no Clipping
}
