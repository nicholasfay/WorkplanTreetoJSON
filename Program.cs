﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Set_UUIDs_for_objects
{

    class Program
    {
	static void Main(string[] args)
	{

	    STEPNCLib.Finder Find = new STEPNCLib.Finder();
	    STEPNCLib.AptStepMaker APT = new STEPNCLib.AptStepMaker();
        StringBuilder builder = new StringBuilder();
        String out_dirFile = args[0];
        String input_file = args[1];

        APT.Open238(input_file);
        Find.Open238(input_file);

	    long wp_id = Find.GetMainWorkplan();
        int depth = 0;
        bool last = false;
        long count = 0;

	    String uu = APT.SetUUID_if_not_set(wp_id);
	    //System.Console.WriteLine("Main Workplan name " + Find.GetExecutableName(wp_id) + " has UUID: " + uu);
	    Mark_plan(wp_id, Find, APT, builder, depth, last, ref count);
	    Mark_pieces(Find, APT, builder);
	    Mark_tools(Find, APT, builder);
	    Mark_technologies(Find, APT, builder);

	    APT.SaveAsModules("hardmoldy_IMTS_signed_uuid.stpnc");

        string output = builder.ToString();
        if (!Directory.Exists(out_dirFile)) //See if the path exists
            Directory.CreateDirectory(Path.GetDirectoryName(out_dirFile)); //Create if not

        using (StreamWriter out_file = //StreamWrite output to file
                new StreamWriter(File.Open(out_dirFile, FileMode.Create)))
        {
            out_file.WriteLine(output);
        }
        //Console.ReadLine();
    }

    //Marks a Workplan or Selective with attributes: ["name" (if available), "base_time", "opt_time" (if available), "distance", "children"] 
	static void Mark_plan (long wp_id, STEPNCLib.Finder Find, STEPNCLib.AptStepMaker APT, StringBuilder builder, int depth, bool last, ref long count)
	{
	    long size = Find.GetNestedExecutableCount(wp_id);
        if (Find.IsWorkplan(wp_id))
        {
            if (count != 0)
            {
                builder.Append(",\n");
                count = 0;
            }

            for (int i = 0; i < depth; i++)
                builder.Append("\t");
            builder.Append("{\"workplan\": {");
            string name = Find.GetExecutableName(wp_id);
            double base_time = Find.GetExecutableBaseTime(wp_id);
            double opt_time = Find.GetExecutableOptimizedTime(wp_id);
            double distance = Find.GetExecutableDistance(wp_id);
            if (name != "")
            {
                builder.Append("\"name\": \"");
                builder.Append(name);
                builder.Append("\", ");
            }
            builder.Append("\"base_time\": ");
            builder.Append(base_time);
            builder.Append(", ");
            if (opt_time != base_time)
            {
                builder.Append("\"opt_time\": ");
                builder.Append(opt_time);
                builder.Append(", ");
            }
            builder.Append("\"distance\": ");
            builder.Append(distance);
            builder.Append(", ");
            builder.Append("\"children\" : [\n");
        }
        else if (Find.IsSelective(wp_id))
        {
            if (count != 0)
            {
                builder.Append(",\n");
                count = 0;
            }

            for (int i = 0; i < depth; i++)
                builder.Append("\t");
            builder.Append("{\"selective\": {");
            string name = Find.GetExecutableName(wp_id);
            double base_time = Find.GetExecutableBaseTime(wp_id);
            double opt_time = Find.GetExecutableOptimizedTime(wp_id);
            double distance = Find.GetExecutableDistance(wp_id);
            if (name != "")
            {
                builder.Append("\"name\": \"");
                builder.Append(name);
                builder.Append("\", ");
            }
            builder.Append("\"base_time\": ");
            builder.Append(base_time);
            builder.Append(", ");
            if (opt_time != base_time)
            {
                builder.Append("\"opt_time\": ");
                builder.Append(opt_time);
                builder.Append(", ");
            }
            builder.Append("\"distance\": ");
            builder.Append(distance);
            builder.Append(", ");
            builder.Append("\"children\" : [\n");
        }
        else {
            Console.WriteLine("ERROR IN STEPNC FILE");
        }
        for (int I = 0; I < size; I++)
        {
            long exe_id = Find.GetNestedExecutableNext(wp_id, I);
            String uu = APT.SetUUID_if_not_set(exe_id);

            String type = Find.GetExecutableType(exe_id);
            //System.Console.WriteLine("Item at " + I + " is a " + type + " has UUID: " + uu);
            if (I == (size - 1))
                last = true;
            else
                last = false;

            if (Find.IsWorkplan(exe_id) || Find.IsSelective(exe_id))
                Mark_plan(exe_id, Find, APT, builder, depth + 1, last, ref count);
            else if (Find.IsWorkingstep(exe_id))
                Mark_step(exe_id, Find, APT, builder, depth + 1, last, ref count);
        }

        for (int i = 0; i < depth; i++)
            builder.Append("\t");
        builder.Append("]}}");
	}

    //Marks a Workingstep with attributes: ["name" (if available), "base_time", "opt_time" (if available), "distance"]
	static void Mark_step(long ws_id, STEPNCLib.Finder Find, STEPNCLib.AptStepMaker APT, StringBuilder builder, int depth, bool last, ref long count)
	{
        string name = Find.GetWorkingstepName2(ws_id);
        double base_time = Find.GetExecutableBaseTime(ws_id);
        double opt_time = Find.GetExecutableOptimizedTime(ws_id);
        double distance = Find.GetExecutableDistance(ws_id);
        for (int i = 0; i < depth; i++)
            builder.Append("\t");
        builder.Append("{\"working_step\": {");
        if (name != "")
        {
            builder.Append("\"name\": \"");
            builder.Append(name);
            builder.Append("\", ");
        }
        builder.Append("\"base_time\": ");
        builder.Append(base_time);
        builder.Append(", ");
        if (opt_time != base_time)
        {
            builder.Append("\"opt_time\": ");
            builder.Append(opt_time);
            builder.Append(", ");
        }
        builder.Append("\"distance\": ");
        builder.Append(distance);
        builder.Append(", ");

        List<long> paths = Find.GetWorkingstepPathAll(ws_id);
        builder.Append("\"toolpaths\" : [\n");
        bool last2 = false;
	    for(int i = 0; i < paths.Count; i++){
            long tp_id = paths[i];
            if(i == (paths.Count - 1))
                last2 = true;
            else
                last2 = false;
		    String uu = APT.SetUUID_if_not_set(tp_id);
            decode_tp(Find, builder, tp_id, depth + 1, last2, ref count);
	    }

        if (last)
        {
            builder.Append(" ]}}\n");
            count = count + 1;
        }
        else
            builder.Append(" ]}},\n");
     }

    static void decode_tp(STEPNCLib.Finder Find, StringBuilder builder, long tp_id, int depth, bool last2, ref long count) {
            //time, distance, speed, feed
            double time = Find.GetExecutableBaseTime(tp_id);
            double distance = Find.GetExecutableDistance(tp_id);
            double feed = Find.GetProcessFeed(tp_id);
            double speed = Find.GetProcessSpeed(tp_id);

            for (int i = 0; i < depth; i++)
                builder.Append("\t");
            builder.Append("{\"toolpath\": {");

            builder.Append("\"base_time\": ");
            builder.Append(time);
            builder.Append(", ");

            builder.Append("\"distance\": ");
            builder.Append(distance);
            builder.Append(", ");

            builder.Append("\"feed_rate\": ");
            builder.Append(feed);
            builder.Append(", ");

            builder.Append("\"speed\": ");
            builder.Append(speed);

            if (last2)
            {
                builder.Append(" }}\n");
                count = count + 1;
            }
            else
                builder.Append(" }},\n");

        }

    /*static void decode_geom(STEPNCLib.Finder Find, StringBuilder builder, long crv_count, long tp_id) {
        for(int i = 0; i < crv_count; i++) {
            bool isArc;
            long crv_id = Find.GetPathCurveNext(tp_id, i, out isArc);
            String type = Find.GetPathCurveType(crv_id);

            Console.WriteLine("Curve type: " + type);
        }
    }*/

	static void Mark_pieces(STEPNCLib.Finder Find, STEPNCLib.AptStepMaker APT, StringBuilder builder)
	{
	    long count = Find.GetWorkpieceCount();

	    for (long i = 0; i < count; i++){
		    long wp_id = Find.GetWorkpieceNext(i);
		    String uu = APT.SetUUID_if_not_set(wp_id);
		    //System.Console.WriteLine("Workpiece at " + wp_id + " has UUID: " + uu);
	    }
	}

	static void Mark_tools(STEPNCLib.Finder Find, STEPNCLib.AptStepMaker APT, StringBuilder builder)
	{
	    long count = Find.GetToolAllCount();

	    for (long i = 0; i < count; i++){
		    long tl_id = Find.GetToolAllNext(i);
		    String uu = APT.SetUUID_if_not_set(tl_id);
		    //System.Console.WriteLine("Tool at " + tl_id + " has UUID: " + uu);
	    }
	}

	static void Mark_technologies(STEPNCLib.Finder Find, STEPNCLib.AptStepMaker APT, StringBuilder builder)
	{
	    long count = Find.GetTechnologyAllCount();
	    double feed, speed;

	    for (long i = 0; i < count; i++){
		    long tech_id = Find.GetTechnologyAllNext(i, out feed, out speed);
		    String uu = APT.SetUUID_if_not_set(tech_id);
		    //System.Console.WriteLine("Tool at " + tech_id + " has UUID: " + uu);
	    }
	}

    }
}
