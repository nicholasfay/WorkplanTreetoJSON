/* $RCSfile: StepNCtoJSON.cs $
 * $Revision: 1.0 $ $Date: 2015/10/13 16:22:59 $
 * Auth: Nicholas Fay (fayn@rpi.edu)
 * 
 * 	Copyright (c) 1991-2015 by STEP Tools Inc.
 * 	All Rights Reserved
 * 
 * 	This software is furnished under a license and may be used and
 * 	copied only in accordance with the terms of such license and with
 * 	the inclusion of the above copyright notice.  This software and
 * 	accompanying written materials or any other copies thereof may
 * 	not be provided or otherwise made available to any other person.
 * 	No title to or ownership of the software is hereby transferred.
 * 
 * 		----------------------------------------
 * 
 *  Convert a STEP-NC file to JSON.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

/*Command Line argument checks for: 
    arg[0]: (true if wants separate file, false if wants to use command line(work in progress)
    arg[1]: (Input .stpnc file name to be used by the program) Must be in same directory as cs file
    arg[2]: (Output file path that ends in specificed text file name e.g. "C:test\test.txt"
*/
namespace JSON
{

    class StepNCtoJSON
    {
        static void Main(string[] args)
        {
            string b = args[0]; //See if the user wants a separate file or not (true = separate, false = console(Not fully implemented))
            bool test;
            if (b == "true")
                test = true;
            else
                test = false;

            // Create a trivial STEP-NC file
            STEPNCLib.Finder Find = new STEPNCLib.Finder();
            string file = args[1];

            Find.Open238(file);

            StringBuilder builder = new StringBuilder();

            long wp_id = Find.GetMainWorkplan(); //Gets main workplan id
            string exe_name = Find.GetExecutableName(wp_id); //Gets main executable name
            int depth = 0; //Depth of tabs to make print look better
            bool last = false; //Boolean to check whether or not its the last element to remove repeating characters
            long last2 = 0; //Counter to be used in order to check whether or not element is last element in children array
            stepThrough(Find, wp_id, test, builder, depth, last, ref last2);

            if (test)
            {
                string output = builder.ToString();
                string out_dirFile = args[2];
                using (StreamWriter out_file =
                    new StreamWriter(File.Open(out_dirFile, FileMode.Create)))
                {
                    out_file.WriteLine(output);
                }
            }
            else
            {
                Console.WriteLine(builder.ToString());
                Console.ReadLine();
            }
        }

        static void stepThrough(STEPNCLib.Finder Find, long wp_id, bool file, StringBuilder builder, int depth, bool last, ref long count)
        {
            if (Find.IsWorkingstep(wp_id)) //Can add NC function functionality to here if needed
            {
                string name = Find.GetWorkingstepName2(wp_id);
                double base_time = Find.GetExecutableBaseTime(wp_id);
                double opt_time = Find.GetExecutableOptimizedTime(wp_id);
                double distance = Find.GetExecutableDistance(wp_id);
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
                if (last)
                {
                    builder.Append(" }}\n");
                    count = count + 1;
                }
                else
                    builder.Append(" }},\n");
            }
            else //Recursive call for Selectives and Workplans
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

                for (int I = 0; I < size; I++)
                {
                    long exe_id = Find.GetNestedExecutableNext(wp_id, I);
                    if (I == (size - 1))
                        last = true;
                    else
                        last = false;
                    if (Find.IsWorkplan(wp_id))
                        stepThrough(Find, exe_id, file, builder, depth + 1, last, ref count);
                    else if (Find.IsSelective(wp_id))
                        stepThrough(Find, exe_id, file, builder, depth + 1, last, ref count);
                    else
                        return;
                }
                for (int i = 0; i < depth; i++)
                    builder.Append("\t");
                builder.Append("]}}");
            }
        }
    }
}