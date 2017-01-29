using System;
using Irony.Parsing;

namespace SmaliVS.Language
{
    [Language("Smali", "1.0", "Smali data format")]
    public class SmaliGrammar : Grammar
    {
        public static SmaliGrammar Instance => (SmaliGrammar)CurrentGrammar ?? new SmaliGrammar();

        #region OpCodes
        public struct OpEntry
        {
            public string OpCode { get; private set; }
            public string Name { get; }
            public string NameNoSyntax
            {
                get
                {
                    var idx = Name.IndexOf(' ');
                    return idx == -1 ? Name : Name.Substring(0, idx);
                }
            }
            public string Description { get; private set; }
            public string Example { get; private set; }
            public OpEntry(string code, string name, string des, string ex) { OpCode = code; Name = name; Description = des; Example = ex; }
        }

        public OpEntry[] OpCodes = new OpEntry[] {
            new OpEntry ("00", "nop", "No operation", "0000 - nop "),
            new OpEntry ("01", "move vx,vy", "Moves the content of vy into vx. Both registers must be in the first 256 register range.", "0110 - move v0, v1Moves v1 into v0."),
            new OpEntry ("02", "move/from16 vx,vy", "Moves the content of vy into vx. vy may be in the 64k register range while vx is one of the first 256 registers.", "0200 1900 - move/from16 v0, v25Moves v25 into v0."),
            new OpEntry("03", "move/16", "", ""),
            new OpEntry("04", "move-wide ", "", ""),
            new OpEntry("05", "move-wide/from16 vx,vy", "Moves a long/double value from vy to vx. vy may be in the 64k register range while wx is one of thefirst 256 registers.", "0516 0000 - move-wide/from16v22, v0Moves v0 into v22."),
            new OpEntry("06", "move-wide/16", "", ""),
            new OpEntry("07", "move-object vx,vy", "Moves the object reference from vy to vx.", "0781 - move-object v1, v8Moves the object reference in v8 to v1."),
            new OpEntry("08", "move-object/from16 vx,vy", "Moves the object reference from vy to vx, vy can address 64k registers and vx can address 256 registers.", "0801 1500 - move-object/from16v1, v21Move the object reference in v21 to v1."),
            new OpEntry("09", "move-object/16", "", ""),
            new OpEntry("0A", "move-result vx", "Move the result value of the previous method invocation into vx.", "0A00 - move-result v0Move the return value of a previous method invocation into v0."),
            new OpEntry("0B", "move-result-wide vx", "Move the long/double result value of the previous method invocation into vx, vx+1.", "0B02 - move-result-wide v2Move the long/double result value of the previous method invocationinto v2,v3."),
            new OpEntry("0C", "move-result-object vx", "Move the result object reference of the previous method invocation into vx.", "0C00 - move-result-object v0"),
            new OpEntry("0D", "move-exception vx", "Move the exception object reference thrown during a method invocation into vx. ", "0D19 - move-exception v25"),
            new OpEntry("0E", "return-void", "Return without a return value", "0E00 - return-void"),
            new OpEntry("0F", "return vx", "Return with vx return value", "0F00 - return v0Returns with return value in v0.      "),
            new OpEntry("10", "return-wide vx", "Return with double/long result in vx, vx+1.", "1000 - return-wide v0Returns with a double/long value in v0,v1."),
            new OpEntry("11", "return-object vx", "Return with vx object reference value.", "1100 - return-object v0Returns with object reference value in v0"),
            new OpEntry("12", "const/4 vx,lit4", "Puts the 4 bit constant into vx", "1221 - const/4 v1, #int2Moves literal 2 into v1. The destination register is in the lower 4 bitin the second byte, the literal 2 is in the higher 4 bit."),
            new OpEntry("13", "const/16 vx,lit16", "Puts the 16 bit constant into vx", "1300 0A00 - const/16 v0, #int 10Puts the literal constant of 10 into v0."),
            new OpEntry("14", "const vx, lit32", "Puts the integer constant into vx", "1400 4E61 BC00 - const v0,#12345678 // #00BC614EMoves literal 12345678 into v0."),
            new OpEntry("15", "const/high16 v0, lit16", "Puts the 16 bit constant into the topmost bits of the register. Used to initialize float values.", "1500 2041 - const/high16 v0,#float 10.0 // #41200000Moves the floating literal of 10.0 into v0. The 16 bit literal in theinstruction carries the top 16 bits of the floating point number."),
            new OpEntry("16", "const-wide/16 vx, lit16", "Puts the integer constant into vx and vx+1 registers, expanding the integer constant into a long constant.", "1600 0A00 - const-wide/16 v0,#long 10Moves literal 10 into v0 and v1 registers."),
            new OpEntry("17", "const-wide/32 vx, lit32", "Puts the 32 bit constant into vx and vx+1 registers, expanding the integer constant into a long constant.", "1702 4e61 bc00 - const-wide/32v2, #long 12345678 // #00bc614ePuts #12345678 into v2 and v3 registers."),
            new OpEntry("18", "const-wide vx, lit64", "Puts the 64 bit constant into vx and vx+1 registers.", "1802 874b 6b5d 54dc 2b00-const-wide v2, #long 12345678901234567 // #002bdc545d6b4b87Puts #12345678901234567 into v2 and v3 registers."),
            new OpEntry("19", "const-wide/high16 vx,lit16", "Puts the 16 bit constant into the highest 16 bit of vx and vx+1 registers. Used to initialize double values.", "1900 2440 - const-wide/high16v0, #double 10.0 // #402400000Puts the double constant of 10.0 into v0 register."),
            new OpEntry("1A", "const-string vx,string_id", "Puts reference to a string constant identified by string_id into vx.", "1A08 0000 - const-string v8 // string@0000Puts reference to string@0000 (entry #0 in the string table) into v8."),
            new OpEntry("1B", "const-string-jumbo", "", ""),
            new OpEntry("1C", "const-class vx,type_id", "Moves the class object of a class identified by type_id (e.g. Object.class) into vx.", "1C00 0100 - const-class v0,Test3 // type@0001Moves reference to Test3.class (entry#1 in the type id table) into "),
            new OpEntry("1D", "monitor-enter vx", "Obtains the monitor of the object referenced by vx.", "1D03 - monitor-enter v3Obtains the monitor of the object referenced by v3."),
            new OpEntry("1E", "monitor-exit", "Releases the monitor of the object referenced by vx.", "1E03 - monitor-exit v3Releases the monitor of the object referenced by v3."),
            new OpEntry("1F", "check-cast vx, type_id", "Checks whether the object reference in vx can be cast to an instance of a class referenced by type_id. Throws ClassCastException if the cast is not possible, continues execution otherwise.", "1F04 0100 - check-cast v4, Test3// type@0001Checks whether the object reference in v4 can be cast to type@0001(entry #1 in the type id table)"),
            new OpEntry("20", "instance-of vx,vy,type_id", "Checks whether vy is instance of a class identified by type_id. Sets vx non-zero if it is, 0 otherwise.", "2040 0100 - instance-of v0, v4,Test3 // type@0001Checks whether the object reference in v4 is an instance of type@0001(entry #1 in the type id table). Sets v0 to non-zero if v4 is instanceof Test3, 0 otherwise."),
            new OpEntry("21", "array-length vx,vy", "Calculates the number of elements of the array referenced by vy and puts the length value into vx.", "2111 - array-length v1, v1Calculates the number of elements of the array referenced by v1 andputs the result into v1."),
            new OpEntry("22", "new-instance vx,type", "Instantiates an object type and puts the reference of the newly created instance into vx.", "2200 1500 - new-instance v0,java.io.FileInputStream // type@0015Instantiates type@0015 (entry #15H in the type table) and puts itsreference into v0."),
            new OpEntry("23", "new-array vx,vy,type_id", "Generates a new array of type_id type and vy element size and puts the reference to the array into vx.", "2312 2500 - new-array v2, v1,char[] // type@0025Generates a new array of type@0025 type and v1 size and puts thereference to the new array into v2."),
            new OpEntry("24", "filled-new-array {parameters},type_id", "Generates a new array of type_id and fills it with the parameters. Reference to the newlygenerated array can be obtained by a move-result-object instruction,immediately following the filled-new-array instruction. ", " 2420 530D 0000 -filled-new-array v0,v0,[I // type@0D53Generates a new array of type@0D53. The array's size will be 2 and bothelements will be filled with the contents of v0 register."),
            new OpEntry("25", "filled-new-array-range {vx..vy},type_id", "Generates a new array of type_id and fills it with a range of parameters. Reference to the newly generated array can be obtained by a move-result-object instruction, immediately following the filled-new-array instruction. ", "2503 0600 1300 -filled-new-array/range v19..v21, [B // type@0006Generates a new array of type@0D53. The array's size will be 3 and theelements will be filled using the v19,v20 and v21 registers    <sup>4</sup>. "),
            new OpEntry("26", "fill-array-datavx,array_data_offset", "Fills the array referenced by vx with the static data. The location of the static data is the sum of the position of the current instruction and the offset", "2606 2500 0000 - fill-array-datav6, 00e6 // +0025Fills the array referenced by v0 with the static data at currentinstruction+25H words location. The offset is expressed as a 32-bitnumber. The static data is stored in the following format:0003 // Table type: static array data0400 // Byte per array element (in this case, 4 byte integers)0300 0000 // Number of elements in the table0100 0000&nbsp; // Element #0: integer 10200 0000 // Element #1: integer 20300 0000 // Element #2: integer3"),
            new OpEntry("27", "throw vx", "Throws an exception object. The reference of the exception object is in vx.", "2700 - throw v0Throws an exception. The exception object reference is in v0."),
            new OpEntry("28", "goto target", "Unconditional jump by short offset.", "28F0 - goto 0005 // -0010Jumps to current position-16 words (hex 10). 0005 is the label of thetarget instruction."),
            new OpEntry("29", "goto/16 target", "Unconditional jump by 16 bit offset.", "2900 0FFE - goto/16 002f // -01f1Jumps to the current position-1F1H words. 002F is the label of thetarget instruction."),
            new OpEntry("2A", "goto/32 target", "", ""),
            new OpEntry("2B", "packed-switch vx,table", "Implements a switch statement where the case constants are close to each other. The instruction uses an index table. vx indexes into this table to find the offset of the instruction for a particular case. If vx falls out of the index table, the execution continues on the next instruction (default case).", "2B02 0C00 0000 - packed-switchv2, 000c // +000cExecute a packed switch according to the switch argument in v2. Theposition of the index table is at current instruction+0CH words. Thetable looks like the following:0001 // Table type: packed switch table0300 // number of elements0000 0000 // element base0500 0000&nbsp; 0: 00000005 // case 0: +000000050700 0000&nbsp; 1: 00000007 // case 1: +000000070900 0000&nbsp; 2: 00000009 // case 2: +00000009"),
            new OpEntry("2C", "sparse-switch vx,table", "Implements a switch statement with sparse case table. The instruction uses a lookup table with case constants and offsets for each case constant. If there is no match in the table, execution continues on the next instruction (default case).", "2C02 0c00 0000 - sparse-switchv2, 000c // +000cExecute a sparse switch according to the switch argument in v2. Theposition of the lookup table is at current instruction+0CH words. Thetable looks like the following.0002 // Table type: sparse switch table0300 // number of elements9cff ffff // first case: -100fa00 0000 // second case constant: 250e803 0000 // third case constant: 10000500 0000 // offset for the first case constant: +50700 0000 // offset for the second case constant: +70900 0000 // offset for the third case constant: +9"),
            new OpEntry("2D", "cmpl-float", "Compares the float values in vy and vz and sets the integer value in vx accordingly.", "2D00 0607 - cmpl-float v0, v6, v7Compares the float values in v6 and v7 then sets v0 accordingly. NaNbias is less-than, the instruction will return -1 if any of theparameters is NaN. "),
            new OpEntry("2E", "cmpg-float vx, vy, vz", "Compares the float values in vy and vz and sets the integer value in vx accordingly.", "2E00 0607 - cmpg-float v0, v6, v7Compares the float values in v6 and v7 then sets v0 accordingly. NaNbias is greater-than, the instruction will return 1 if any of theparameters is NaN. "),
            new OpEntry("2F", "cmpl-double vx,vy,vz", "Compares the double values in vy and vz and sets the integer value in vx accordingly.", "2F19 0608 - cmpl-double v25, v6,v8Compares the double values in v6,v7 and v8,v9 and sets v25 accordingly.NaN bias is less-than, the instruction will return -1 if any of theparameters is NaN. "),
            new OpEntry("30", "cmpg-double vx, vy, vz", "Compares the double values in vy and vz and sets the integer value in vx accordingly.", "3000 080A - cmpg-double v0, v8,v10Compares the double values in v8,v9 and v10,v11 then sets v0accordingly. NaN bias is greater-than, the instruction will return 1 ifany of the parameters is NaN. "),
            new OpEntry("31", "cmp-long vx, vy, vz", "Compares the long values in vy and vz and sets the integer value in vx accordingly.", "3100 0204 - cmp-long v0, v2, v4Compares the long values in v2 and v4 then sets v0 accordingly."),
            new OpEntry("32", "if-eq vx,vy,target", "Jumps to target if vx == vy. vx and vy are integer values. ", "32b3 6600 - if-eq v3, v11, 0080// +0066Jumps to the current position+66H words if v3==v11. 0080 is the labelof the target instruction."),
            new OpEntry("33", "if-ne vx,vy,target", "Jumps to target if vx != vy. vx and vy are integer values. ", "33A3 1000 - if-ne v3, v10, 002c// +0010Jumps to the current position+10H words if v3!=v10. 002c is the labelof the target instruction."),
            new OpEntry("34", "if-lt vx,vy,target", "Jumps to target is vx < vy. vx and vy are integer values. ", "3432 CBFF - if-lt v2, v3, 0023// -0035Jumps to the current position-35H words if v2&lt;v3. 0023 is the labelof the target instruction."),
            new OpEntry("35", "if-ge vx, vy,target", "Jumps to target if vx> = vy. vx and vy are integer values. ", "3510 1B00 - if-ge v0, v1, 002b//+001bJumps to the current position+1BH words if v0&gt;=v1. 002b is the labelof the target instruction."),
            new OpEntry("36", "if-gt vx,vy,target", "Jumps to target if vx > vy. vx and vy are integer values. ", "3610 1B00 - if-ge v0, v1, 002b//+001bJumps to the current position+1BH words if v0&gt;v1. 002b is the labelof the target instruction."),
            new OpEntry("37", "if-le vx,vy,target", "Jumps to target if vx <= vy. vx and vy are integer values. ", "3756 0B00 - if-le v6, v5, 0144// +000bJumps to the current position+0BH words if v6&lt;=v5. 0144 is the labelof the target instruction."),
            new OpEntry("38", "if-eqz vx,target", "Jumps to target if vx == 0. vx is an integer value. ", "3802 1900 - if-eqz v2, 0038 //+0019Jumps to the current position+19H words if v2==0. 0038 is the labelof the target instruction."),
            new OpEntry("39", "if-nez vx,target", "Checks vx and jumps if vx isnonzero    <sup>2</sup>. ", "3902 1200 - if-nez v2, 0014 //+0012Jumps to current position+18 words (hex 12) if v2 is nonzero. 0014 isthe label of the target instruction."),
            new OpEntry("3A", "if-ltz vx,target", "Checks vx and jumps if vx < 0.", "3A00 1600 - if-ltz v0, 002d //+0016Jumps to the current position+16H words if v0&lt;0. 002d is the labelof the target instruction. "),
            new OpEntry("3B", "if-gez vx,target", "Checks vx and jumps if vx >= 0.", "3B00 1600 - if-gez v0, 002d //+0016Jumps to the current position+16H words if v0 &gt;=0. 002d is the labelof the target instruction."),
            new OpEntry("3C", "if-gtz vx,target", "Checks vx and jumps if vx > 0.", "3C00 1D00 - if-gtz v0, 004a //+001dJumps to the current position+1DH words if v0&gt;0. 004A is the labelof the target instruction. "),
            new OpEntry("3D", "if-lez vx,target", "Checks vx and jumps if vx <= 0.. ", "3D00 1D00 - if-lez v0, 004a //+001dJumps to the current position+1DH words if v0&lt;=0. 004A is the labelof the target instruction."),
            //new OpEntry("3E", "unused_3E", "", ""),
            //new OpEntry("3F", "unused_3F", "", ""),
            //new OpEntry("40", "unused_40", "", ""),
            //new OpEntry("41", "unused_41", "", ""),
            //new OpEntry("42", "unused_42", "", ""),
            //new OpEntry("43", "unused_43", "", ""),
            new OpEntry("44", "aget vx,vy,vz", "Gets an integer value of an object reference array into vx. The array is referenced by vy and is indexed by vz.", "4407 0306 - aget v7, v3, v6Gets an integer array element. The array is referenced by v3and the element is indexed by v6. The element will be put into v7."),
            new OpEntry("45", "aget-wide vx,vy,vz", "Gets a long/double value of long/double array into vx, vx+1. The array is referenced by vy and is indexed by vz.", "4505 0104 - aget-wide v5, v1, v4Gets a long/double array element. The array is referenced by v1and the element is indexed by v4. The element will be put into v5,v6."),
            new OpEntry("46", "aget-object vx,vy,vz", "Gets an object reference value of an object reference array into vx. The array is referenced by vy and is indexed by vz.", "4602 0200 - aget-object v2, v2,v0Gets an object reference array element. The array is referenced by v2and the element is indexed by v0. The element will be put into v2."),
            new OpEntry("47", "aget-boolean vx,vy,vz", "Gets a boolean value of a boolean array into vx. The array is referenced by vy and is indexed by vz. ", "4700 0001 - aget-boolean v0, v0,v1 Gets a boolean array element. The array is referenced by v0and the element is indexed by v1. The element will be put into v0. "),
            new OpEntry("48", "aget-byte vx,vy,vz", "Gets a byte value of a byte array into vx. The array is referenced by vy and is indexed by vz.", "4800 0001 - aget-byte v0, v0, v1Gets a byte array element. The array is referenced by v0and the element is indexed by v1. The element will be put into v0."),
            new OpEntry("49", "aget-char vx, vy,vz", "Gets a char value of a character array into vx. The element is indexed by vz, the array object is referenced by vy.", "4905 0003 - aget-char v5, v0, v3Gets a character array element. The array is referenced by v0 and theelement is indexed by v3. The element will be put into v5."),
            new OpEntry("4A", "aget-short vx,vy,vz", "Gets a short value of ashort array into vx. The element is indexed by vz, the array object is referenced by vy.", "4A00 0001 - aget-short v0, v0, v1Gets a short array element. The array is referenced by v0 and theelement is indexed by v1. The element will be put into v0."),
            new OpEntry("4B", "aput vx,vy,vz", "Puts the integer value in vx into an element of an integer array. The element is indexed by vz, the array object is referenced by vy.", "4B00 0305 - aput v0, v3, v5Puts the integer value in v2 into an integer arrayreferenced by v0. The target array element is indexed by v1."),
            new OpEntry("4C", "aput-wide vx,vy,vz", "Puts the double/long value in vx, vx+1 into a double/long array. The array is referenced by vy, the element is indexed by vz.", "4C05 0104 - aput-wide v5, v1, v4Puts the double/long value in v5,v6 into a double/long arrayreferenced by v1. The target array element is indexed by v4."),
            new OpEntry("4D", "aput-object vx,vy,vz", "Puts the object reference value in vx into an element of an object reference array. The element is indexed by vz, the array object is referenced by vy.", "4D02 0100 - aput-object v2, v1,v0Puts the object reference value in v2 into an object reference arrayreferenced by v0. The target array element is indexed by v1."),
            new OpEntry("4E", "aput-boolean vx,vy,vz", "Puts the boolean value in vx into an element of a boolean array. The element is indexed by vz, the array object is referenced by vy.", "4E01 0002 - aput-boolean v1, v0,v2Puts the boolean value in v1 into an object reference arrayreferenced by v0. The target array element is indexed by v2."),
            new OpEntry("4F", "aput-byte vx,vy,vz", "Puts the byte value in vx into an element of a byte array. The element is indexed by vz, the array object is referenced by vy.", "4F02 0001 - aput-byte v2, v0, v1Puts the boolean value in v2 into a byte arrayreferenced by v0. The target array element is indexed by v1."),
            new OpEntry("50", "aput-char vx,vy,vz", "Puts the char value in vx into an element of a character array. The element is indexed by vz, the array object is referenced by vy.", "5003 0001 - aput-char v3, v0, v1Puts the character value in v3 into a character array referenced by v0.The target array element is indexed by v1. "),
            new OpEntry("51", "aput-short vx,vy,vz", "Puts the short value in vx into an element of a short array. The element is indexed by vz, the array object is referenced by vy.", "5102 0001 - aput-short v2, v0, v1Puts the short value in v2 into a character array referenced by v0.The target array element is indexed by v1."),
            new OpEntry("52", "iget vx, vy, field_id", "Reads an instance field into vx. The instance is referenced by vy.", "5210 0300 - iget v0, v1,Test2.i6:I // field@0003Reads field@0003 into v0 (entry #3 in the field id table). The instanceis referenced by v1."),
            new OpEntry("53", "iget-wide vx,vy,field_id", "Reads an instance field into vx. The instance is referenced by vy.", "5320 0400 - iget-wide v0, v2,Test2.l0:J // field@0004Reads field@0004 into v0 and v1 registers (entry #4 in the field idtable). The instance is referenced by v2."),
            new OpEntry("54", "iget-object vx,vy,field_id", "Reads an object reference instance field into vx. The instance is referenced by vy.", "iget-object v1, v2,LineReader.fis:Ljava/io/FileInputStream; // field@0002Reads field@0002 into v1&nbsp; (entry #2 in the field idtable). The instance is referenced by v2."),
            new OpEntry("55", "iget-boolean vx,vy,field_id", "Reads a boolean instance field into vx. The instance is referenced by vy.", "55FC 0000 - iget-boolean v12,v15, Test2.b0:Z // field@0000Reads the boolean field@0000 into v12 register (entry #0 in the fieldid table). The instance is referenced by v15."),
            new OpEntry("56", "iget-byte vx,vy,field_id", "Reads a byte instance field into vx. The instance is referenced by vy.", "5632 0100 - iget-byte v2, v3,Test3.bi1:B // field@0001Reads the char field@0001 into v2 register (entry #1 in the fieldid table). The instance is referenced by v3."),
            new OpEntry("57", "iget-char vx,vy,field_id", "Reads a char instance field into vx. The instance is referenced by vy.", "5720 0300 - iget-char v0, v2,Test3.ci1:C // field@0003Reads the char field@0003 into v0 register (entry #3 in the fieldid table). The instance is referenced by v2."),
            new OpEntry("58", "iget-short vx,vy,field_id", "Reads a short instance field into vx. The instance is referenced by vy.", "5830 0800 - iget-short v0, v3,Test3.si1:S // field@0008Reads the short field@0008 into v0 register (entry #8 in the fieldid table). The instance is referenced by v3."),
            new OpEntry("59", "iput vx,vy, field_id", "Puts vx into an instance field. The instance is referenced by vy.", "5920 0200 - iput v0,v2,Test2.i6:I // field@0002Stores v0 into field@0002 (entry #2 in the field id table). Theinstance is referenced by v2."),
            new OpEntry("5A", "iput-wide vx,vy, field_id", "Puts the wide value located in vx and vx+1 registers into an instance field. The instance is referenced by vy.", "5A20 0000 - iput-wide v0,v2,Test2.d0:D // field@0000 Stores the wide value in v0, v1 registers into field@0000 (entry #0 inthe field id table). The instance is referenced by v2."),
            new OpEntry("5B", "iput-object vx,vy,field_id", "Puts the object reference in vx into an instance field. The instance is referenced by vy.", "5B20 0000 - iput-object v0, v2,LineReader.bis:Ljava/io/BufferedInputStream; // field@0000Stores the object reference in v0 into field@0000 (entry #0 in thefield table). The instance is referenced by v2."),
            new OpEntry("5C", "iput-boolean vx,vy, field_id", "Puts the boolean value located in vx into an instance field. The instance is referenced by vy.", "5C30 0000 - iput-boolean v0, v3,Test2.b0:Z // field@0000Puts the boolean value in v0 into field@0000 (entry #0 in the field idtable). The instance is referenced by v3."),
            new OpEntry("5D", "iput-byte vx,vy,field_id", "Puts the byte value located in vx into an instance field. The instance is referenced by vy.", "5D20 0100 - iput-byte v0, v2,Test3.bi1:B // field@0001Puts the boolean value in v0 into field@0001 (entry #1 in the field idtable). The instance is referenced by v2."),
            new OpEntry("5E", "iput-char vx,vy,field_id", "Puts the char value located in vx into an instance field. The instance is referenced by vy.", "5E20 0300 - iput-char v0, v2,Test3.ci1:C // field@0003Puts the char value in v0 into field@0003 (entry #3 in the field idtable). The instance is referenced by v2."),
            new OpEntry("5F", "iput-short vx,vy,field_id", "Puts the short value located in vx into an instance field. The instance is referenced by vy.", "5F21 0800 - iput-short v1, v2,Test3.si1:S // field@0008Puts the short value in v1 into field@0008 (entry #8 in the field idtable). The instance is referenced by v2."),
            new OpEntry("60", "sget vx,field_id", "Reads the integer field identified by the field_id into vx.", "6000 0700 - sget v0, Test3.is1:I// field@0007Reads field@0007 (entry #7 in the field id table) into v0."),
            new OpEntry("61", "sget-wide vx, field_id", "Reads the static field identified by the field_id into vx and vx+1 registers.", "6100 0500 - sget-wide v0,Test2.l1:J // field@0005Reads field@0005 (entry #5 in the field id table) into v0 and v1registers."),
            new OpEntry("62", "sget-object vx,field_id", "Reads the object reference field identified by the field_id into vx.", "6201 0C00 - sget-object v1,Test3.os1:Ljava/lang/Object; // field@000cReads field@000c (entry #CH in the field id table) into v1."),
            new OpEntry("63", "sget-boolean vx,field_id", "Reads the boolean static field identified by the field_id into vx.", "6300 0C00 - sget-boolean v0,Test2.sb:Z // field@000cReads boolean field@000c (entry #12 in the field id table) into v0."),
            new OpEntry("64", "sget-byte vx,field_id", "Reads the byte static field identified by the field_id into vx.", "6400 0200 - sget-byte v0,Test3.bs1:B // field@0002Reads byte field@0002 (entry #2 in the field id table) into v0."),
            new OpEntry("65", "sget-char vx,field_id", "Reads the char static field identified by the field_id into vx.", "6500 0700 - sget-char v0,Test3.cs1:C // field@0007Reads byte field@0007 (entry #7 in the field id table) into v0."),
            new OpEntry("66", "sget-short vx,field_id", "Reads the short static field identified by the field_id into vx.", "6600 0B00 - sget-short v0,Test3.ss1:S // field@000bReads short field@000b (entry #BH in the field id table) into v0."),
            new OpEntry("67", "sput vx, field_id", "Puts vx into a static field.", "6700 0100 - sput v0, Test2.i5:I// field@0001Stores v0 into field@0001 (entry #1 in the field id table)."),
            new OpEntry("68", "sput-wide vx, field_id", "Puts vx and vx+1 into a static field.", "6800 0500 - sput-wide v0,Test2.l1:J // field@0005Puts the long value in v0 and v1 into the field@0005 static field(entry #5 in the field id table)."),
            new OpEntry("69", "sput-object vx,field_id", "Puts object reference in vx into a static field.", "6900 0c00 - sput-object v0,Test3.os1:Ljava/lang/Object; // field@000cPuts the object reference value in v0 into the field@000c static field(entry #CH in the field id table)."),
            new OpEntry("6A", "sput-boolean vx,field_id", "Puts boolean value in vx into a static field.", "6A00 0300 - sput-boolean v0,Test3.bls1:Z // field@0003Puts the byte value in v0 into the field@0003 static field(entry #3 in the field id table)."),
            new OpEntry("6B", "sput-byte vx,field_id", "Puts byte value in vx into a static field.", "6B00 0200 - sput-byte v0,Test3.bs1:B // field@0002Puts the byte value in v0 into the field@0002 static field(entry #2 in the field id table)."),
            new OpEntry("6C", "sput-char vx,field_id", "Puts char value in vx into a static field.", "6C01 0700 - sput-char v1,Test3.cs1:C // field@0007Puts the char value in v1 into the field@0007 static field(entry #7 in the field id table)."),
            new OpEntry("6D", "sput-short vx,field_id", "Puts short value in vx into a static field.", "6D00 0B00 - sput-short v0,Test3.ss1:S // field@000bPuts the short value in v0 into the field@000b static field(entry #BH in the field id table)."),
            new OpEntry("6E", "invoke-virtual { parameters} , methodtocall", "Invokes a virtual method with parameters.", "6E53 0600 0421 - invoke-virtual v4, v0, v1, v2, v3, Test2.method5:(IIII)V // method@0006Invokes the 6th method in the method table with the followingarguments: v4 is the \"this\" instance, v0, v1, v2, and v3 are the methodparameters. The method has 5 arguments (4 MSB bits of the second byte)    <sup>5</sup>. "),
            new OpEntry("6F", "invoke-super { parameter} , methodtocall", "Invokes the virtual method of the immediate parent class.", "6F10 A601 0100 invoke-super v1,java.io.FilterOutputStream.close:()V // method@01a6Invokes method@01a6 with one parameter, v1."),
            new OpEntry("70", "invoke-direct {parameters }, methodtocall", "Invokes a method with parameters without the virtual method resolution.", "7010 0800 0100 - invoke-direct v1, java.lang.Object.&lt;init&gt;:()V // method@0008Invokes the 8th method in the method table with just one parameter, v1is the \"this\" instance    <sup>5</sup>. "),
            new OpEntry("71", "invoke-static {parameters}, methodtocall", "Invokes a static method with parameters.", "7110 3400 0400 - invoke-static v4, java.lang.Integer.parseInt:( Ljava/lang/String;)I // method@0034Invokes method@34 static method. The method is called with oneparameter, v4    <sup>5</sup>. "),
            new OpEntry("72", "invoke-interface {parameters}, methodtocall", "Invokes an interface method.", "7240 2102 3154 invoke-interface v1, v3, v4, v5, mwfw.IReceivingProtocolAdapter.receivePackage:(ILjava/lang/String;Ljava/io/InputStream;)Z // method@0221Invokes method@221 interface method using parameters in v1,v3,v4 and v5    <sup>5</sup>. "),
            //new OpEntry("73", "unused_73", "", ""),
            new OpEntry("74", "invoke-virtual/range {vx..vy}, methodtocall", "Invokes virtual method with arange of registers. The instruction specifies the first register and the number of registers to be passed to the method.", "7403 0600 1300 - invoke-virtual v19..v21, Test2.method5:(IIII)V // method@0006Invokes the 6th method in the method table with the followingarguments: v19 is the \"this\" instance, v20 and v21 are the methodparameters.&nbsp; "),
            new OpEntry("75", "invoke-super/range", "Invokes the virtual method of the immediate parent class. The instruction specifies the first register and the number of registers to be passed to the method.", "7501 A601 0100 invoke-super v1,java.io.FilterOutputStream.close:()V // method@01a6Invokes method@01a6 with one parameter, v1. "),
            new OpEntry("76", "invoke-direct/range {vx..vy}, methodtocall", "Invokes direct method with arange of registers. The instruction specifies the first register andthe number of registers to be passed to the method.", "7603 3A00 1300 -invoke-direct/range v19..21,java.lang.Object.&lt;init&gt;:()V //method@003aInvokes method@3A with 1 parameters (second byte of theinstruction=03). The parameter is stored in v19 (5th,6th bytes of theinstruction)."),
            new OpEntry("77", "invoke-static/range {vx..vy},methodtocall", "Invokes static method with arange of registers. The instruction specifies the first register and the number of registers to be passed to the method.", "7703 3A00 1300 -invoke-static/range v19..21,java.lang.Integer.parseInt:(Ljava/lang/String;)I // method@0034Invokes method@3A with 1 parameters (second byte of theinstruction=03). The parameter is stored in v19 (5th,6th bytes of theinstruction). "),
            new OpEntry("78", "invoke-interface-range", "Invokes an interface method with a range of registers. The instruction specifies the first register and the number of registers to be passed to the method.", "7840 2102 0100 invoke-interface v1..v4, mwfw.IReceivingProtocolAdapter.receivePackage:(ILjava/lang/String;Ljava/io/InputStream;)Z // method@0221Invokes method@221 interface method using parameters in v1..v4. "),
            //new OpEntry("79", "unused_79", "", ""),
            //new OpEntry("7A", "unused_7A", "", ""),
            new OpEntry("7B", "neg-int vx,vy", "Calculates vx =- vy.", "7B01 - neg-int v1,v0Calculates -v0 and stores the result in v1."),
            new OpEntry("7C", "not-int vx,vy", "", ""),
            new OpEntry("7D", "neg-long vx,vy", "Calculates vx, vx+1 =- (vy, vy+1)", "7D02 - neg-long v2,v0Calculates -(v0,v1) and stores the result into (v2,v3)"),
            new OpEntry("7E", "not-long vx,vy", "", ""),
            new OpEntry("7F", "neg-float vx,vy", "Calculates vx =- vy", "7F01 - neg-float v1,v0Calculates -v0 and stores the result into v1."),
            new OpEntry("80", "neg-double vx,vy", "Calculates vx, vx+1 =- (vy,vy+1) ", "8002 - neg-double v2,v0Calculates -(v0,v1) and stores the result into (v2,v3)"),
            new OpEntry("81", "int-to-long vx, vy", "Converts the integer in vy into a long in vx, vx+1.", "8106 - int-to-long v6, v0Converts an integer in v0 into a long in v6,v7."),
            new OpEntry("82", "int-to-float vx, vy", "Converts the integer in vx into a float in vx.", "8206 - int-to-float v6, v0Converts the integer in v0 into a float in v6."),
            new OpEntry("83", "int-to-double vx, vy", "Converts the integer in vy into the double in vx, vx+1.", "8306 - int-to-double v6, v0Converts the integer in v0 into a double in v6,v7"),
            new OpEntry("84", "long-to-int vx,vy", "Converts the long value in vy, vy+1 into an integer in vx.", "8424 - long-to-int v4, v2Converts the long value in v2,v3 into an integer value in v4."),
            new OpEntry("85", "long-to-float vx, vy", "Converts the long value in vy, vy+1 into a float in vx.", "8510 - long-to-float v0, v1Convcerts the long value in v1,v2 into a float value in v0."),
            new OpEntry("86", "long-to-double vx, vy", "Converts the long value in vy, vy+1 into a double value in vx, vx+1.", "8610 - long-to-double v0, v1Converts the long value in v1,v2 into a double value in v0,v1."),
            new OpEntry("87", "float-to-int vx, vy", "Converts the float value in vy into an integer value in vx.", "8730 - float-to-int v0, v3Converts the float value in v3 into an integer value in v0."),
            new OpEntry("88", "float-to-long vx,vy", "Converts the float value in vy into a long value in vx.", "8830 - float-to-long v0, v3Converts the float value in v3 into a long value in v0,v1."),
            new OpEntry("89", "float-to-double vx, vy", "Converts the float value in vyinto a double value in vx,vx+1.", "8930 - float-to-double v0, v3Converts the float value in v3 into a double value in v0,v1."),
            new OpEntry("8A", "double-to-int vx, vy", "Converts the double value in vy, vy+1 into an integer value in vx.", "8A40&nbsp; - double-to-int v0, v4Converts the double value in v4,v5 into an integer value in v0."),
            new OpEntry("8B", "double-to-long vx, vy", "Converts the double value in vy, vy+1 into a long value in vx, vx+1.", "8B40 - double-to-long v0, v4Converts the double value in v4,v5 into a long value in v0,v1."),
            new OpEntry("8C", "double-to-float vx, vy", "Converts the double value in vy, vy+1 into a float value in vx.", "8C40 - double-to-float v0, v4Converts the double value in v4,v5 into a float value in v0,v1."),
            new OpEntry("8D", "int-to-byte vx,vy", "Converts the int value in vy to a byte value and stores it in vx.", "8D00 - int-to-byte v0, v0Converts the integer in v0 into a byte and puts the byte value into v0."),
            new OpEntry("8E", "int-to-char vx,vy", "Converts the int value in vy to a char value and stores it in vx.", "8E33&nbsp; - int-to-char v3, v3Converts the integer in v3 into a char and puts the char value into v3."),
            new OpEntry("8F", "int-to-short vx,vy", "Converts the int value in vy to a short value and stores it in vx. ", "8F00 - int-to-short v0, v0Converts the integer in v0 into a short and puts the short value intov3."),
            new OpEntry("90", "add-int vx,vy,vz", "Calculates vy+vz and puts the result into vx.", "9000 0203 - add-int v0, v2, v3Adds v3 to v2 and puts the result into v0    <sup>4</sup>. "),
            new OpEntry("91", "sub-int vx,vy,vz", "Calculates vy-vz and puts the result into vx.", "9100 0203 - sub-int v0, v2, v3Subtracts v3 from v2 and puts the result into v0."),
            new OpEntry("92", "mul-int vx, vy, vz", "Multiplies vz with wy and puts the result int vx.", "9200 0203 - mul-int v0,v2,v3Multiplies v2 with w3 and puts the result into v0"),
            new OpEntry("93", "div-int vx,vy,vz", "Divides vy with vz and puts the result into vx.", "9303 0001 - div-int v3, v0, v1Divides v0 with v1 and puts the result into v3."),
            new OpEntry("94", "rem-int vx,vy,vz", "Calculates vy % vz and puts the result into vx. ", "9400 0203 - rem-int v0, v2, v3Calculates v3 % v2 and puts the result into v0. "),
            new OpEntry("95", "and-int vx, vy, vz", "Calculates vy AND vz and puts the result into vx.", "9503 0001 - and-int v3, v0, v1Calculates v0 AND v1 and puts the result into v3."),
            new OpEntry("96", "or-int vx, vy, vz", "Calculates vy OR vz and puts the result into vx.", "9603 0001 - or-int v3, v0, v1Calculates v0 OR v1 and puts the result into v3."),
            new OpEntry("97", "xor-int vx, vy, vz", "Calculates vy XOR vz and puts the result into vx.", "9703 0001 - xor-int v3, v0, v1Calculates v0 XOR v1 and puts the result into v3."),
            new OpEntry("98", "shl-int vx, vy, vz", "Shift vy left by the positions specified by vz and store the result into vx.", "9802 0001 - shl-int v2, v0, v1Shift v0 left by the positions specified by v1 and store the result inv2."),
            new OpEntry("99", "shr-int vx, vy, vz", "Shift vy right by the positions specified by vz and store the result into vx.", "9902 0001 - shr-int v2, v0, v1Shift v0 right by the positions specified by v1 and store the result inv2."),
            new OpEntry("9A", "ushr-int vx, vy, vz", "Unsigned shift right>>> vy by the positions specified by vz and store the result into vx.", "9A02 0001 - ushr-int v2, v0, v1Unsigned shift v0 right by the positions specified by v1 and store theresult in v2."),
            new OpEntry("9B", "add-long vx, vy, vz", "Adds vy to vz and puts the result into vx.", "9B00 0305 - add-long v0, v3, v5The long value in v3,v4 is added to the value in v5,v6 and the resultis stored in v0,v1."),
            new OpEntry("9C", "sub-long vx,vy,vz", "Calculates vy - vz and puts the result into vx.", "9C00 0305 - sub-long v0, v3, v5Subtracts the long value in v5,v6 from the long value in v3,v4 and putsthe result into v0,v1."),
            new OpEntry("9D", "mul-long vx,vy,vz", "Calculates vy * vz and puts the result into vx.", "9D00 0305 - mul-long v0, v3, v5Multiplies the long value in v5,v6 with the long value in v3,v4 andputsthe result into v0,v1. "),
            new OpEntry("9E", "div-long vx, vy, vz", "Calculates vy / vz and puts the result into vx.", "9E06 0002 - div-long v6, v0, v2Divides the long value in v0,v1 with the long value in v2,v3 and pustthe result into v6,v7."),
            new OpEntry("9F", "rem-long vx,vy,vz", "Calculates vy % vz and puts the result into vx.", "9F06 0002 - rem-long v6, v0, v2Calculates v0,v1 %&nbsp; v2,v3 and putsthe result into v6,v7. "),
            new OpEntry("A0", "and-long vx, vy, vz", "Calculates the vy AND vz and puts the result into vx.", "A006 0002 - and-long v6, v0, v2Calculates v0,v1 AND v2,v3 and puts the result into v6,v7."),
            new OpEntry("A1", "or-long vx, vy, vz", "Calculates the vy OR vz and puts the result into vx.", "A106 0002 - or-long v6, v0, v2Calculates v0,v1 OR v2,v3 and puts the result into v6,v7."),
            new OpEntry("A2", "xor-long vx, vy, vz", "Calculates the vy XOR vz and puts the result into vx.", "A206 0002 - xor-long v6, v0, v2Calculates v0,v1 XOR v2,v3 and puts the result into v6,v7."),
            new OpEntry("A3", "shl-long vx, vy, vz", "Shifts left vy by vz positions and stores the result in vx.", "A302 0004 - shl-long v2, v0, v4Shift v0,v1 by postions specified by v4 and puts the result into v2,v3."),
            new OpEntry("A4", "shr-long vx,vy,vz", "Shifts right vy by vz positions and stores the result in vx.", "A402 0004 - shr-long v2, v0, v4Shift v0,v1 by postions specified by v4 and puts the result into v2,v3."),
            new OpEntry("A5", "ushr-long vx, vy, vz", "Unsigned shifts right vy by vz positions and stores the result in vx.", "A502 0004 - ushr-long v2, v0, v4Unsigned shift v0,v1 by postions specified by v4 and puts the resultinto v2,v3."),
            new OpEntry("A6", "add-float vx,vy,vz", "Adds vy to vz and puts the result into vx.", "A600 0203 - add-float v0, v2, v3Adds the floating point numbers in v2 and v3 and puts the result intov0."),
            new OpEntry("A7", "sub-float vx,vy,vz", "Calculates vy-vz and puts the result into vx. ", "A700 0203 - sub-float v0, v2, v3Calculates v2-v3 and puts the result intov0. "),
            new OpEntry("A8", "mul-float vx, vy, vz", "Multiplies vy with vz and puts the result into vx.", "A803 0001 - mul-float v3, v0, v1Multiplies v0 with v1 and puts the result into v3."),
            new OpEntry("A9", "div-float vx, vy, vz", "Calculates vy / vz and puts the result into vx.", "A903 0001 - div-float v3, v0, v1Divides v0 with v1 and puts the result into v3."),
            new OpEntry("AA", "rem-float vx,vy,vz", "Calculates vy % vz and puts the result into vx. ", "AA03 0001 - rem-float v3, v0, v1Calculates v0 %&nbsp; v1 and puts the result into v3. "),
            new OpEntry("AB", "add-double vx,vy,vz", "Adds vy to vz and puts the result into vx.", "AB00 0305 - add-double v0, v3, v5Adds the double value in v5,v6 registers to the double value in v3,v4registers and places the result&nbsp; in v0,v1 registers."),
            new OpEntry("AC", "sub-double vx,vy,vz", "Calculates vy - vz and puts the result into vx.", "AC00 0305 - sub-double v0, v3, v5Subtracts the value in v5,v6 from the value in v3,v4 and puts theresult into v0,v1."),
            new OpEntry("AD", "mul-double vx, vy, vz", "Multiplies vy with vz and puts the result into vx.", "AD06 0002 - mul-double v6, v0, v2Multiplies the double value in v0,v1 with the double value in v2,v3 andputs the result into v6,v7."),
            new OpEntry("AE", "div-double vx, vy, vz", "Calculates vy / vz and puts the result into vx.", "AE06 0002 - div-double v6, v0, v2Divides the double value in v0,v1 with the double value in v2,v3 andputs the result into v6,v7."),
            new OpEntry("AF", "rem-double vx,vy,vz", "Calculates vy % vz and puts the result into vx.", "AF06 0002 - rem-double v6, v0, v2Calculates v0,v1 % v2,v3 andputs the result into v6,v7. "),
            new OpEntry("B0", "add-int/2addr vx,vy", "Adds vy to vx.", "B010 - add-int/2addr v0,v1Adds v1 to v0."),
            new OpEntry("B1", "sub-int/2addr vx,vy", "Calculates vx-vy and puts the result into vx.", "B140 - sub-int/2addr v0, v4Subtracts v4 from v0 and puts the result into v0."),
            new OpEntry("B2", "mul-int/2addr vx,vy", "Multiplies vx with vy.", "B210 - mul-int/2addr v0, v1Multiples v0 with v1 and puts the result into v0."),
            new OpEntry("B3", "div-int/2addr vx,vy", "Divides vx with vy and puts the result into vx.", "B310 - div-int/2addr v0, v1Divides v0 with v1 and puts the result into v0."),
            new OpEntry("B4", "rem-int/2addr vx,vy", "Calculates vx % vy and puts the result into vx ", "B410 - rem-int/2addr v0, v1&nbsp;Calculates v0 % v1 and puts the result into v0. "),
            new OpEntry("B5", "and-int/2addr vx, vy", "Calculates vx AND vy and puts the result into vx.", "B510 - and-int/2addr v0, v1Calculates v0 AND v1 and puts the result into v0."),
            new OpEntry("B6", "or-int/2addr vx, vy", "Calculates vx OR vy and puts the result into vx.", "B610 - or-int/2addr v0, v1Calculates v0 OR v1 and puts the result into v0."),
            new OpEntry("B7", "xor-int/2addr vx, vy", "Calculates vx XOR vy and puts the result into vx.", "B710&nbsp; - xor-int/2addr v0, v1Calculates v0 XOR v1 and puts the result into v0."),
            new OpEntry("B8", "shl-int/2addr vx, vy", "Shifts vx left by vy positions.", "B810 - shl-int/2addr v0, v1Shift v0 left by v1 positions."),
            new OpEntry("B9", "shr-int/2addr vx, vy", "Shifts vx right by vy positions.", "B910 - shr-int/2addr v0, v1Shift v0 right by v1 positions."),
            new OpEntry("BA", "ushr-int/2addr vx, vy", "Unsigned shift right>>> vx by the positions specified by vy.", "BA10 - ushr-int/2addr v0, v1Unsigned shift v0 by the positions specified by v1."),
            new OpEntry("BB", "add-long/2addr vx,vy", "Adds vy to vx.", "BB20 - add-long/2addr v0, v2Adds the long value in v2,v3 registers to the long value in v0,v1registers."),
            new OpEntry("BC", "sub-long/2addr vx,vy", "Calculates vx-vy and puts the result into vx.", "BC70 - sub-long/2addr v0, v7Subtracts the long value in v7,v8 from the long value in v0,v1 and putsthe result into v0,v1."),
            new OpEntry("BD", "mul-long/2addr vx,vy", "Calculates vx*vy and puts the result into vx.", "BD70 - mul-long/2addr v0, v7Multiplies the long value in v7,v8 with the long value in v0,v1 andputsthe result into v0,v1. "),
            new OpEntry("BE", "div-long/2addr vx, vy", "Calculates vx/vy and puts the result into vx.", "BE20 - div-long/2addr v0, v2Divides the long value in v0,v1 with the long value in v2,v3 and putsthe result into v0,v1"),
            new OpEntry("BF", "rem-long/2addr vx,vy", "Calculates vx % vy and puts the result into vx.", "BF20 - rem-long/2addr v0, v2Calculates v0,v1 % v2,v3 and putsthe result into v0,v1 "),
            new OpEntry("C0", "and-long/2addr vx, vy", "Calculates vx AND vy and puts the result into vx.", "C020 - and-long/2addr v0, v2Calculates v0,v1 OR v2,v3 and puts the result into v0,v1."),
            new OpEntry("C1", "or-long/2addr vx, vy", "Calculates vx OR vy and puts the result into vx.", "C120&nbsp; - or-long/2addr v0, v2Calculates v0,v1 OR v2,v3 and puts the result into v0,v1."),
            new OpEntry("C2", "xor-long/2addr vx, vy", "Calculates vx XOR vy and puts the result into vx.", "C220 - xor-long/2addr v0, v2Calculates v0,v1 XOR v2,v3 and puts the result into v0,v1."),
            new OpEntry("C3", "shl-long/2addr vx, vy", "Shifts left the value in vx, vx+1 by the positions specified by vy and stores the result in vx, vx+1.", "C320 - shl-long/2addr v0, v2Shifts left v0,v1 by the positions specified by v2."),
            new OpEntry("C4", "shr-long/2addr vx, vy", "Shifts right the value invx, vx+1 by the positions specified by vy and stores the result invx, vx+1.", "C420 - shr-long/2addr v0, v2Shifts right v0,v1 by the positions specified by v2."),
            new OpEntry("C5", "ushr-long/2addr vx, vy", "Unsigned shifts right the valuein vx, vx+1 by the positions specified by vy and stores the result invx, vx+1.", "C520 - ushr-long/2addr v0, v2Unsigned shifts right v0,v1 by the positions specified by v2."),
            new OpEntry("C6", "add-float/2addr vx,vy", "Adds vy to vx. ", "C640 - add-float/2addr v0,v4Adds v4 to v0."),
            new OpEntry("C7", "sub-float/2addr vx,vy", "Calculates vx - vy and stores the result in vx.", "C740 - sub-float/2addr v0,v4Adds v4 to v0. "),
            new OpEntry("C8", "mul-float/2addr vx, vy", "Multiplies vx with vy.", "C810 - mul-float/2addr v0, v1Multiplies v0 with v1."),
            new OpEntry("C9", "div-float/2addr vx, vy", "Calculates vx / vy and puts the result into vx.", "C910 - div-float/2addr v0, v1Divides v0 with v1 and puts the result into v0."),
            new OpEntry("CA", "rem-float/2addr vx,vy", "Calculates vx / vy and puts the result into vx.", "CA10 - rem-float/2addr v0, v1&nbsp;Calculates v0 % v1 and puts the result into v0. "),
            new OpEntry("CB", "add-double/2addr vx, vy", "Adds vy to vx.", "CB70 - add-double/2addr v0, v7Adds v7 to v0."),
            new OpEntry("CC", "sub-double/2addr vx, vy", "Calculates vx - vy and puts the result into vx.", "CC70 - sub-double/2addr v0, v7Subtracts the value in v7,v8 from the value in v0,v1 and puts theresult into v0,v1."),
            new OpEntry("CD", "mul-double/2addr vx, vy", "Multiplies vx with vy.", "CD20 - mul-double/2addr v0, v2Multiplies the double value in v0,v1 with the double value in v2,v3 andputs the result into v0,v1."),
            new OpEntry("CE", "div-double/2addr vx, vy", "Calculates vx / vy and puts the result into vx.", "CE20 - div-double/2addr v0, v2Divides the double value in v0,v1 with the double value in v2,v3 andputs the value into v0,v1."),
            new OpEntry("CF", "rem-double/2addr vx,vy", "Calculates vx % vy and puts the result into vx.", "CF20 - rem-double/2addr v0, v2&nbsp;Calculates&nbsp; v0,v1 %&nbsp; v2,v3 andputs the value into v0,v1. "),
            new OpEntry("D0", "add-int/lit16 vx,vy,lit16", "Adds vy to lit16 and stores the result into vx.", "D001 D204 - add-int/lit16 v1,v0, #int 1234 // #04d2Adds v0 to literal 1234 and stores the result into v1."),
            new OpEntry("D1", "sub-int/lit16 vx,vy,lit16", "Calculates vy - lit16 and stores the result into vx.", "D101 D204 - sub-int/lit16 v1,v0, #int 1234 // #04d2Calculates v0 - literal 1234 and stores the result into v1."),
            new OpEntry("D2", "mul-int/lit16 vx,vy,lit16", "Calculates vy * lit16 and stores the result into vx.", "D201 D204 - mul-int/lit16 v1,v0, #int 1234 // #04d2Calculates v0 * literal 1234 and stores the result into v1. "),
            new OpEntry("D3", "div-int/lit16 vx,vy,lit16", "Calculates vy / lit16 and stores the result into vx.", "D301 D204 - div-int/lit16 v1,v0, #int 1234 // #04d2Calculates v0 / literal 1234 and stores the result into v1. "),
            new OpEntry("D4", "rem-int/lit16 vx,vy,lit16", "Calculates vy % lit16 and stores the result into vx.", "D401 D204 - rem-int/lit16 v1,v0, #int 1234 // #04d2Calculates v0 % literal 1234 and stores the result into v1. "),
            new OpEntry("D5", "and-int/lit16 vx,vy,lit16", "Calculates vy AND lit16 and stores the result into vx.", "D501 D204 - and-int/lit16 v1,v0, #int 1234 // #04d2Calculates v0 AND literal 1234 and stores the result into v1. "),
            new OpEntry("D6", "or-int/lit16 vx,vy,lit16", "Calculates vy OR lit16 and stores the result into vx.", "D601 D204 - or-int/lit16 v1,v0, #int 1234 // #04d2Calculates v0 OR literal 1234 and stores the result into v1. "),
            new OpEntry("D7", "xor-int/lit16 vx,vy,lit16", "Calculates vy XOR lit16 and stores the result into vx. ", "D701 D204 - xor-int/lit16 v1,v0, #int 1234 // #04d2Calculates v0 XOR literal 1234 and stores the result into v1. "),
            new OpEntry("D8", "add-int/lit8 vx,vy,lit8", "Adds vy to lit8 and stores the result into vx.", "D800 0201 - add-int/lit8 v0,v2,#int1Adds literal 1 to v2 and stores the result into v0."),
            new OpEntry("D9", "sub-int/lit8 vx,vy,lit8", "Calculates vy - lit8 and stores the result into vx.", "D900 0201 - sub-int/lit8 v0,v2,#int1Calculates v2-1 and stores the result into v0. "),
            new OpEntry("DA", "mul-int/lit8 vx,vy,lit8", "Multiplies vy with lit8 8-bit literal constant and puts the result into vx.", "DA00 0002 - mul-int/lit8 v0,v0,#int2Multiplies v0 with literal 2 and puts the result into v0."),
            new OpEntry("DB", "div-int/lit8 vx,vy,lit8", "Calculates vy / lit8 and stores the result into vx.", "DB00 0203 - mul-int/lit8 v0,v2,#int3Calculates v2/3 and stores the result into v0. "),
            new OpEntry("DC", "rem-int/lit8 vx,vy,lit8", "Calculates vy % lit8 and stores the result into vx.", "DC00 0203 - rem-int/lit8 v0,v2,#int3Calculates v2 % 3 and stores the result into v0. "),
            new OpEntry("DD", "and-int/lit8 vx,vy,lit8", "Calculates vy AND lit8 andstores theresult into vx.", "DD00 0203 - and-int/lit8 v0,v2,#int3Calculates v2 AND 3 and stores the result into v0. "),
            new OpEntry("DE", "or-int/lit8 vx, vy, lit8", "Calculates vy OR lit8 and puts the result into vx.", "DE00 0203 - or-int/lit8 v0, v2,#int 3Calculates v2 OR literal 3 and puts the result into v0."),
            new OpEntry("DF", "xor-int/lit8 vx, vy, lit8", "Calculates vy XOR lit8 and puts the result into vx.", "DF000203&nbsp;&nbsp;&nbsp;&nbsp; |&nbsp; 0008: xor-int/lit8 v0, v2, #int 3Calculates v2 XOR literal 3 and puts the result into v0."),
            new OpEntry("E0", "shl-int/lit8 vx, vy, lit8", "Shift v0 left by the bit positions specified by the literal constant and put the result into vx.", "E001 0001 - shl-int/lit8 v1, v0,#int 1Shift v0 left by 1 position and put the result into v1."),
            new OpEntry("E1", "shr-int/lit8 vx, vy, lit8", "Shift v0 right by the bit positions specified by the literal constant and put the result into vx.", "E101 0001 - shr-int/lit8 v1, v0,#int 1Shift v0 right by 1 position and put the result into v1."),
            new OpEntry("E2", "ushr-int/lit8 vx, vy, lit8", "Unsigned right shift of v0>>> by the bit positions specified by the literal constant and put the result into vx.", "E201 0001 - ushr-int/lit8 v1,v0, #int 1Unsigned shift v0 right by 1 position and put the result into v1."),
            //new OpEntry("E3", "unused_E3", "", ""),
            //new OpEntry("E4", "unused_E4", "", ""),
            //new OpEntry("E5", "unused_E5", "", ""),
            //new OpEntry("E6", "unused_E6", "", ""),
            //new OpEntry("E7", "unused_E7", "", ""),
            //new OpEntry("E8", "unused_E8", "", ""),
            //new OpEntry("E9", "unused_E9", "", ""),
            //new OpEntry("EA", "unused_EA", "", ""),
            //new OpEntry("EB", "unused_EB", "", ""),
            //new OpEntry("EC", "unused_EC", "", ""),
            //new OpEntry("ED", "unused_ED", "", ""),
            new OpEntry("EE", "execute-inline {parameters}, inline ID", "Executes the inline method identified by inline ID.", "EE20 0300 0100 - execute-inline v1, v0, inline #0003Executes inline method #3 using v1 as \"this\" and passing one parameterin v0."),
            //new OpEntry("EF", "unused_EF", "", ""),
            new OpEntry("F0", "invoke-direct-empty", "Stands as a placeholder for pruned empty methods like Object.<init>. This acts as nop during normal execution.", "F010 F608 0000 -invoke-direct-empty v0, Ljava/lang/Object;.&lt;init&gt;:()V //method@08f6Replacement for the empty method java/lang/Object;&lt;init&gt;."),
            //new OpEntry("F1", "unused_F1", "", ""),
            new OpEntry("F2", "iget-quick vx,vy,offset", "Gets the value stored at offset in vy instance's data area to vx.", "F221 1000 - iget-quick v1, v2,[obj+0010]Gets the value at offset 0CH of the instance pointed by v2 and storesthe object reference in v1."),
            new OpEntry("F3", "iget-wide-quick vx,vy,offset", "Gets the object reference value stored at offset in vy instance's data area to vx, vx+1.", "F364 3001 - iget-wide-quick v4,v6, [obj+0130]Gets the value at offset 130H of the instance pointed by v6 and storesthe object reference in v4,v5."),
            new OpEntry("F4", "iget-object-quick vx,vy,offset", "Gets the object reference value stored at offset in vy instance's data area to vx.", "F431 0C00 - iget-object-quickv1, v3, [obj+000c]Gets the object reference value at offset 0CH of the instance pointedby v3 and stores the object reference in v1."),
            new OpEntry("F5", "iput-quick vx,vy,offset", "Puts the value stored in vx to offset in vy instance's data area.", "F521 1000&nbsp; - iput-quick v1,v2, [obj+0010]Puts the object reference value in v1 to offset 10H of the instancepointed by v2."),
            new OpEntry("F6", "iput-wide-quick vx,vy,offset", "Puts the value stored in vx, vx+1 to offset in vy instance's data area.", "F652 7001 - iput-wide-quick v2,v5, [obj+0170]Puts the value in v2,v3 to offset 170H of the instance pointed by v5."),
            new OpEntry("F7", "iput-object-quick vx,vy,offset", "Puts the object reference value stored in vx to offset in vy instance's data area to vx.", "F701 4C00 - iput-object-quickv1, v0, [obj+004c]Puts the object reference value in v1 to offset 0CH of the instancepointed by v3."),
            new OpEntry("F8", "invoke-virtual-quick {parameters},vtable offset", "Invokes a virtual method using the vtable of the target object.", "F820 B800 CF00 -invoke-virtual-quick v15, v12, vtable #00b8Invokes a virtual method. The target object instance is pointed by v15and vtable entry #B8 points to the method to be called. v12 is aparameter to the method call."),
            new OpEntry("F9", "invoke-virtual-quick/range {parameter range},vtable offset", "Invokes a virtual method using the vtable of the target object.", "F906 1800 0000 -invoke-virtual-quick/range v0..v5,vtable #0018Invokes a method using the vtable of theinstance pointed by v0. v1..v5 registers are parameters to the methodcall."),
            new OpEntry("FA", "invoke-super-quick {parameters},vtable offset", "Invokes a virtual method in the target object's immediate parent class using the vtable of that parent class.", "FA40 8100 3254&nbsp; -invoke-super-quick v2, v3, v4, v5, vtable #0081Invokes a method using the vtable of the immediate parent class ofinstance pointed by v2. v3, v4 and v5 registers are parameters to themethodcall."),
            new OpEntry("FB", "invoke-super-quick/range {register range},vtable offset", "Invokes a virtual method in the target object's immediate parent class using the vtable of that parent class.", "F906 1B00 0000 -invoke-super-quick/range v0..v5, vtable #001bInvokes a method using the vtable of the immediate parent class ofinstance pointed by v0. v1..v5 registers are parameters to the methodcall."),
            //new OpEntry("FC", "unused_FC", "", ""),
            //new OpEntry("FD", "unused_FD", "", ""),
            //new OpEntry("FE", "unused_FE", "", ""),
            //new OpEntry("FF", "unused_FF", "", "")
        };
        #endregion

        public SmaliGrammar()
        {
            //Terminals
            var sString = new StringLiteral("string", "\"");
            var sNumber = new NumberLiteral("number", NumberOptions.AllowLetterAfter);
            var sHexNumber = new NumberLiteral("hexNumber", NumberOptions.AllowLetterAfter);
            sHexNumber.AddPrefix("0x", NumberOptions.Hex);
            sHexNumber.AddPrefix("-0x", NumberOptions.Hex);
            sHexNumber.AddSuffix("L", TypeCode.Int64, TypeCode.UInt64);
            sHexNumber.AddSuffix("t", TypeCode.Byte);
            sHexNumber.AddSuffix("s", TypeCode.Int16);
            sNumber.DefaultIntTypes = sHexNumber.DefaultIntTypes = new[] { TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 };
            sNumber.EditorInfo = new TokenEditorInfo(TokenType.Literal, TokenColor.Number, TokenTriggers.None);
            sHexNumber.EditorInfo = new TokenEditorInfo(TokenType.Literal, TokenColor.Number, TokenTriggers.None);

            var comma = ToTerm(",");
            var sIdent = new IdentifierTerminal("Identifier") { Options = IdOptions.NameIncludesPrefix };
            var sComment = new CommentTerminal("comment", "#", "\r", "\n");
            var sLocTarget = new IdentifierTerminal("TargetLoc");
            sLocTarget.AddPrefix(":", IdOptions.None);
            var sTargetLocation = new NonTerminal("TargetLocation") { Rule = ":" + sIdent };

            //Nonterminals

            var sReg = new IdentifierTerminal("Register");
            var sThisTerm = new NonTerminal("thisterm") { Rule = ToTerm("this") + "$" + sNumber };
            var sAccessTerm = new NonTerminal("accessterm") { Rule = ToTerm("access") + "$" + sNumber };

            var sParams = new NonTerminal("Params");
            sParams.Rule = MakeListRule(sParams, comma, sReg, TermListOptions.AllowEmpty);

            var sOpAccessorList = new NonTerminal("accessor");
            var sOpAccessor = new NonTerminal("accessorList");
            sOpAccessorList.Rule = Keyword("final") | Keyword("synthetic") | Keyword("public") | Keyword("private") | Keyword("static") | Keyword("protected");
            sOpAccessor.Rule = MakePlusRule(sOpAccessor, sOpAccessorList);

            var addClass = new NumberLiteral("classExtra");
            addClass.AddPrefix("$", NumberOptions.IntOnly);

            var sOpFullType = new NonTerminal("fulltype");
            sOpFullType.Rule = MakeListRule(sOpFullType, ToTerm("/") | ToTerm("$"), sIdent, TermListOptions.StarList) + addClass.Q() + ";";

            var termBoolean = ToTerm("Z", "Boolean");
            var termByte = ToTerm("B", "Byte");
            var termShort = ToTerm("S", "Short");
            var termChar = ToTerm("C", "Char");
            var termInt = ToTerm("I", "Int");
            var termLong = ToTerm("J", "Long");
            var termFloat = ToTerm("F", "Float");
            var termDouble = ToTerm("D", "Double");
            var termClass = ToTerm("L", "Class");
            var termVoid = ToTerm("V", "Void");

            // Allow a char after.
            termBoolean.AllowAlphaAfterKeyword = termByte.AllowAlphaAfterKeyword = termShort.AllowAlphaAfterKeyword =
                termChar.AllowAlphaAfterKeyword = termInt.AllowAlphaAfterKeyword = termLong.AllowAlphaAfterKeyword =
                termFloat.AllowAlphaAfterKeyword = termDouble.AllowAlphaAfterKeyword = termClass.AllowAlphaAfterKeyword =
                termVoid.AllowAlphaAfterKeyword = true;

            var sOpClassValueType = new NonTerminal("classValueType");
            var sOpValueType = new NonTerminal("valuetype");
            sOpClassValueType.Rule = termClass + sOpFullType;
            var arrayList = new NonTerminal("arrayList");
            arrayList.Rule = MakeStarRule(arrayList, ToTerm("["));

            sOpValueType.Rule = arrayList + (termBoolean | termByte | termShort | termChar | termInt | termLong | termFloat | termDouble | termVoid | sOpClassValueType);

            var sMethodParams = new NonTerminal("MethodParams");
            sMethodParams.Rule = MakeStarRule(sMethodParams, sOpValueType);
            //sMethodParams.Rule = MakeListRule(sMethodParams, Empty, , TermListOptions.AllowEmpty);
            var sMethodSignature = new NonTerminal("MethodSignature")
            {
                Rule = (ToTerm("<init>") | "<clinit>" | sAccessTerm | sIdent) + "(" + sMethodParams + ")" + sOpValueType
            };

            //     sOpField.Rule = "." + sField + sOpAccessor + (sThisTerm | sIdent) + ":" + sOpValueType;
            var termValueField = new NonTerminal("valueField")
            {
                Rule = ((sThisTerm | ("$" + sIdent) | sIdent) + ":" + sOpValueType) | sMethodSignature
            };

            var termValueFullAccessor = new NonTerminal("fullvalueaccessor")
            {
                Rule = sOpClassValueType + "->" + termValueField
            };



            var sLine = new NonTerminal("Line");
            var sLines = new NonTerminal("Lines");

            var sOpNop = MakeRule("nop");

            #region Move Rules

            var sOpMove = MakeRule("move", sReg, comma, sReg);
            var sOpMoveFrom16 = MakeRule("move/from16", sReg, comma, sReg);
            var sOpMove16 = MakeRule("move/16", sReg, comma, sReg);
            var sOpMoveWide = MakeRule("move-wide", sReg, comma, sReg);
            var sOpMoveWideFrom16 = MakeRule("move-wide/from16", sReg, comma, sReg);
            var sOpMoveWide16 = MakeRule("move-wide/16", sReg, comma, sReg);
            var sOpMoveObject = MakeRule("move-object", sReg, comma, sReg);
            var sOpMoveObjectFrom16 = MakeRule("move-object/from16", sReg, comma, sReg);
            var sOpMoveObject16 = MakeRule("move-object/16", sReg, comma, sReg);
            var sOpMoveResult = MakeRule("move-result", sReg);
            var sOpMoveResultWide = MakeRule("move-result-wide", sReg);
            var sOpMoveResultObject = MakeRule("move-result-object", sReg);
            var sOpMoveException = MakeRule("move-exception", sReg);

            #endregion

            #region Return Rules

            var sOpReturnVoid = MakeRule("return-void");
            var sOpReturn = MakeRule("return", sReg);
            var sOpReturnWide = MakeRule("return-wide", sReg);
            var sOpReturnObject = MakeRule("return-object", sReg);

            #endregion

            #region Const Rules   

            var sOpConst4 = MakeRule("const/4", sReg, comma, sHexNumber);
            var sOpConst16 = MakeRule("const/16", sReg, comma, sHexNumber);
            var sOpConst = MakeRule("const", sReg, comma, sHexNumber);
            var sOpConstHigh16 = MakeRule("const/high16", sReg, comma, sHexNumber);
            var sOpConstWide16 = MakeRule("const-wide/16", sReg, comma, sHexNumber);
            var sOpConstWide32 = MakeRule("const-wide/32", sReg, comma, sHexNumber);
            var sOpConstWide = MakeRule("const-wide", sReg, comma, sHexNumber);
            var sOpConstWideHigh16 = MakeRule("const-wide/high16", sReg, comma, sHexNumber);
            var sOpConstString = MakeRule("const-string", sReg, comma, sString);
            var sOpConstStringJumbo = MakeRule("const-string-jumbo");
            var sOpConstClass = MakeRule("const-class", sReg, comma, sOpClassValueType);

            #endregion

            #region Misc Rules

            var sOpMonitorEnter = MakeRule("monitor-enter", sReg);
            var sOpMonitorExit = MakeRule("monitor-exit");
            var sOpCheckCast = MakeRule("check-cast", sReg, comma, sOpValueType);
            var sOpInstanceOf = MakeRule("instance-of", sReg, comma, sReg, comma, sOpValueType);
            var sOpArrayLength = MakeRule("array-length", sReg, comma, sReg);
            var sOpNewInstance = MakeRule("new-instance", sReg, comma, sOpValueType);
            var sOpNewArray = MakeRule("new-array", sReg, comma, sReg, comma, sOpValueType);
            var sOpFilledNewArray = MakeRule("filled-new-array", "{", sParams, "}", comma, sOpValueType);
            var sOpFilledNewArrayRange = MakeRule("filled-new-array-range", "{", sParams, "}", comma, sOpValueType);
            var sOpFillArrayData = MakeRule("fill-array-data", sReg, comma, sLocTarget);
            var sOpThrow = MakeRule("throw", sReg);
            var sOpGoto = MakeRule("goto", sLocTarget);
            var sOpGoto16 = MakeRule("goto/16", sLocTarget);
            var sOpGoto32 = MakeRule("goto/32", sLocTarget);

            #endregion

            #region Switch\Jump Rules

            var sOpPackedSwitch = MakeRule("packed-switch", sReg, comma, sLocTarget);
            var sOpSparseSwitch = MakeRule("sparse-switch", sReg, comma, sLocTarget);
            var sOpCmplFloat = MakeRule("cmpl-float", sReg, comma, sReg, comma, sReg);
            var sOpCmpgFloat = MakeRule("cmpg-float", sReg, comma, sReg, comma, sReg);
            var sOpCmplDouble = MakeRule("cmpl-double", sReg, comma, sReg, comma, sReg);
            var sOpCmpgDouble = MakeRule("cmpg-double", sReg, comma, sReg, comma, sReg);
            var sOpCmpLong = MakeRule("cmp-long", sReg, comma, sReg, comma, sReg);
            var sOpIfEq = MakeRule("if-eq", sReg, comma, sReg, comma, sLocTarget);
            var sOpIfNe = MakeRule("if-ne", sReg, comma, sReg, comma, sLocTarget);
            var sOpIfLt = MakeRule("if-lt", sReg, comma, sReg, comma, sLocTarget);
            var sOpIfGe = MakeRule("if-ge", sReg, comma, sReg, comma, sLocTarget);
            var sOpIfGt = MakeRule("if-gt", sReg, comma, sReg, comma, sLocTarget);
            var sOpIfLe = MakeRule("if-le", sReg, comma, sReg, comma, sLocTarget);
            var sOpIfEqz = MakeRule("if-eqz", sReg, comma, sLocTarget);
            var sOpIfNez = MakeRule("if-nez", sReg, comma, sLocTarget);
            var sOpIfLtz = MakeRule("if-ltz", sReg, comma, sLocTarget);
            var sOpIfGez = MakeRule("if-gez", sReg, comma, sLocTarget);
            var sOpIfGtz = MakeRule("if-gtz", sReg, comma, sLocTarget);
            var sOpIfLez = MakeRule("if-lez", sReg, comma, sLocTarget);

            #endregion

            #region Get\Put Rules

            var sOpAGet = MakeRule("aget", sReg, comma, sReg, comma, sReg);
            var sOpAGetWide = MakeRule("aget-wide", sReg, comma, sReg, comma, sReg);
            var sOpAGetObject = MakeRule("aget-object", sReg, comma, sReg, comma, sReg);
            var sOpAGetBoolean = MakeRule("aget-boolean", sReg, comma, sReg, comma, sReg);
            var sOpAGetByte = MakeRule("aget-byte", sReg, comma, sReg, comma, sReg);
            var sOpAGetChar = MakeRule("aget-char", sReg, comma, sReg, comma, sReg);
            var sOpAGetShort = MakeRule("aget-short", sReg, comma, sReg, comma, sReg);
            var sOpAPut = MakeRule("aput", sReg, comma, sReg, comma, sReg);
            var sOpAPutWide = MakeRule("aput-wide", sReg, comma, sReg, comma, sReg);
            var sOpAPutObject = MakeRule("aput-object", sReg, comma, sReg, comma, sReg);
            var sOpAPutBoolean = MakeRule("aput-boolean", sReg, comma, sReg, comma, sReg);
            var sOpAPutByte = MakeRule("aput-byte", sReg, comma, sReg, comma, sReg);
            var sOpAPutChar = MakeRule("aput-char", sReg, comma, sReg, comma, sReg);
            var sOpAPutShort = MakeRule("aput-short", sReg, comma, sReg, comma, sReg);
            var sOpIGet = MakeRule("iget", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIGetWide = MakeRule("iget-wide", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIGetObject = MakeRule("iget-object", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIGetBoolean = MakeRule("iget-boolean", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIGetByte = MakeRule("iget-byte", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIGetChar = MakeRule("iget-char", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIGetShort = MakeRule("iget-short", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIPut = MakeRule("iput", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIPutWide = MakeRule("iput-wide", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIPutObject = MakeRule("iput-object", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIPutBoolean = MakeRule("iput-boolean", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIPutByte = MakeRule("iput-byte", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIPutChar = MakeRule("iput-char", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpIPutShort = MakeRule("iput-short", sReg, comma, sReg, comma, termValueFullAccessor);
            var sOpSGet = MakeRule("sget", sReg, comma, termValueFullAccessor);
            var sOpSGetWide = MakeRule("sget-wide", sReg, comma, termValueFullAccessor);
            var sOpSGetObject = MakeRule("sget-object", sReg, comma, termValueFullAccessor);
            var sOpSGetBoolean = MakeRule("sget-boolean", sReg, comma, termValueFullAccessor);
            var sOpSGetByte = MakeRule("sget-byte", sReg, comma, termValueFullAccessor);
            var sOpSGetChar = MakeRule("sget-char", sReg, comma, termValueFullAccessor);
            var sOpSGetShort = MakeRule("sget-short", sReg, comma, termValueFullAccessor);
            var sOpSPut = MakeRule("sput", sReg, comma, termValueFullAccessor);
            var sOpSPutWide = MakeRule("sput-wide", sReg, comma, termValueFullAccessor);
            var sOpSPutObject = MakeRule("sput-object", sReg, comma, termValueFullAccessor);
            var sOpSPutBoolean = MakeRule("sput-boolean", sReg, comma, termValueFullAccessor);
            var sOpSPutByte = MakeRule("sput-byte", sReg, comma, termValueFullAccessor);
            var sOpSPutChar = MakeRule("sput-char", sReg, comma, termValueFullAccessor);
            var sOpSPutShort = MakeRule("sput-short", sReg, comma, termValueFullAccessor);

            #endregion

            #region Invoke Rules

            var sOpInvokeVirtual = MakeRule("invoke-virtual", "{", sParams, "}", comma, termValueFullAccessor);
            var sOpInvokeSuper = MakeRule("invoke-super", "{", sParams, "}", comma, termValueFullAccessor);
            var sOpInvokeDirect = MakeRule("invoke-direct", "{", sParams, "}", comma, termValueFullAccessor);
            var sOpInvokeStatic = MakeRule("invoke-static", "{", sParams, "}", comma, termValueFullAccessor);
            var sOpInvokeInterface = MakeRule("invoke-interface", "{", sParams, "}", comma, termValueFullAccessor);
            var sOpInvokeVirtualRange = MakeRule("invoke-virtual/range", "{", sReg, "..", sReg, "}", comma, termValueFullAccessor);
            var sOpInvokeSuperRange = MakeRule("invoke-super/range", "{", sReg, "..", sReg, "}", comma, termValueFullAccessor);
            var sOpInvokeDirectRange = MakeRule("invoke-direct/range", "{", sReg, "..", sReg, "}", comma, termValueFullAccessor);
            var sOpInvokeStaticRange = MakeRule("invoke-static/range", "{", sReg, "..", sReg, "}", comma, termValueFullAccessor);
            var sOpInvokeInterfaceRange = MakeRule("invoke-interface/range", "{", sReg, "..", sReg, "}", comma, termValueFullAccessor);

            #endregion

            #region Math Op Rules

            var sOpNegInt = MakeRule("neg-int", sReg, comma, sReg);
            var sOpNotInt = MakeRule("not-int", sReg, comma, sReg);
            var sOpNegLong = MakeRule("neg-long", sReg, comma, sReg);
            var sOpNotLong = MakeRule("not-long", sReg, comma, sReg);
            var sOpNegFloat = MakeRule("neg-float", sReg, comma, sReg);
            var sOpNegDouble = MakeRule("neg-double", sReg, comma, sReg);
            var sOpIntToLong = MakeRule("int-to-long", sReg, comma, sReg);
            var sOpIntToFloat = MakeRule("int-to-float", sReg, comma, sReg);
            var sOpIntToDouble = MakeRule("int-to-double", sReg, comma, sReg);
            var sOpLongToInt = MakeRule("long-to-int", sReg, comma, sReg);
            var sOpLongToFloat = MakeRule("long-to-float", sReg, comma, sReg);
            var sOpLongToDouble = MakeRule("long-to-double", sReg, comma, sReg);
            var sOpFloatToInt = MakeRule("float-to-int", sReg, comma, sReg);
            var sOpFloatToLong = MakeRule("float-to-long", sReg, comma, sReg);
            var sOpFloatToDouble = MakeRule("float-to-double", sReg, comma, sReg);
            var sOpDoubleToInt = MakeRule("double-to-int", sReg, comma, sReg);
            var sOpDoubleToLong = MakeRule("double-to-long", sReg, comma, sReg);
            var sOpDoubleToFloat = MakeRule("double-to-float", sReg, comma, sReg);
            var sOpIntToByte = MakeRule("int-to-byte", sReg, comma, sReg);
            var sOpIntToChar = MakeRule("int-to-char", sReg, comma, sReg);
            var sOpIntToShort = MakeRule("int-to-short", sReg, comma, sReg);
            var sOpAddInt = MakeRule("add-int", sReg, comma, sReg, comma, sReg);
            var sOpSubInt = MakeRule("sub-int", sReg, comma, sReg, comma, sReg);
            var sOpMulInt = MakeRule("mul-int", sReg, comma, sReg, comma, sReg);
            var sOpDivInt = MakeRule("div-int", sReg, comma, sReg, comma, sReg);
            var sOpRemInt = MakeRule("rem-int", sReg, comma, sReg, comma, sReg);
            var sOpAndInt = MakeRule("and-int", sReg, comma, sReg, comma, sReg);
            var sOpOrInt = MakeRule("or-int", sReg, comma, sReg, comma, sReg);
            var sOpXorInt = MakeRule("xor-int", sReg, comma, sReg, comma, sReg);
            var sOpShlInt = MakeRule("shl-int", sReg, comma, sReg, comma, sReg);
            var sOpShrInt = MakeRule("shr-int", sReg, comma, sReg, comma, sReg);
            var sOpUshrInt = MakeRule("ushr-int", sReg, comma, sReg, comma, sReg);
            var sOpAddLong = MakeRule("add-long", sReg, comma, sReg, comma, sReg);
            var sOpSubLong = MakeRule("sub-long", sReg, comma, sReg, comma, sReg);
            var sOpMulLong = MakeRule("mul-long", sReg, comma, sReg, comma, sReg);
            var sOpDivLong = MakeRule("div-long", sReg, comma, sReg, comma, sReg);
            var sOpRemLong = MakeRule("rem-long", sReg, comma, sReg, comma, sReg);
            var sOpAndLong = MakeRule("and-long", sReg, comma, sReg, comma, sReg);
            var sOpOrLong = MakeRule("or-long", sReg, comma, sReg, comma, sReg);
            var sOpXorLong = MakeRule("xor-long", sReg, comma, sReg, comma, sReg);
            var sOpShlLong = MakeRule("shl-long", sReg, comma, sReg, comma, sReg);
            var sOpShrLong = MakeRule("shr-long", sReg, comma, sReg, comma, sReg);
            var sOpUshrLong = MakeRule("ushr-long", sReg, comma, sReg, comma, sReg);
            var sOpAddFloat = MakeRule("add-float", sReg, comma, sReg, comma, sReg);
            var sOpSubFloat = MakeRule("sub-float", sReg, comma, sReg, comma, sReg);
            var sOpMulFloat = MakeRule("mul-float", sReg, comma, sReg, comma, sReg);
            var sOpDivFloat = MakeRule("div-float", sReg, comma, sReg, comma, sReg);
            var sOpRemFloat = MakeRule("rem-float", sReg, comma, sReg, comma, sReg);
            var sOpAddDouble = MakeRule("add-double", sReg, comma, sReg, comma, sReg);
            var sOpSubDouble = MakeRule("sub-double", sReg, comma, sReg, comma, sReg);
            var sOpMulDouble = MakeRule("mul-double", sReg, comma, sReg, comma, sReg);
            var sOpDivDouble = MakeRule("div-double", sReg, comma, sReg, comma, sReg);
            var sOpRemDouble = MakeRule("rem-double", sReg, comma, sReg, comma, sReg);
            var sOpAddInt2Addr = MakeRule("add-int/2addr", sReg, comma, sReg);
            var sOpSubInt2Addr = MakeRule("sub-int/2addr", sReg, comma, sReg);
            var sOpMulInt2Addr = MakeRule("mul-int/2addr", sReg, comma, sReg);
            var sOpDivInt2Addr = MakeRule("div-int/2addr", sReg, comma, sReg);
            var sOpRemInt2Addr = MakeRule("rem-int/2addr", sReg, comma, sReg);
            var sOpAndInt2Addr = MakeRule("and-int/2addr", sReg, comma, sReg);
            var sOpOrInt2Addr = MakeRule("or-int/2addr", sReg, comma, sReg);
            var sOpXorInt2Addr = MakeRule("xor-int/2addr", sReg, comma, sReg);
            var sOpShlInt2Addr = MakeRule("shl-int/2addr", sReg, comma, sReg);
            var sOpShrInt2Addr = MakeRule("shr-int/2addr", sReg, comma, sReg);
            var sOpUshrInt2Addr = MakeRule("ushr-int/2addr", sReg, comma, sReg);
            var sOpAddLong2Addr = MakeRule("add-long/2addr", sReg, comma, sReg);
            var sOpSubLong2Addr = MakeRule("sub-long/2addr", sReg, comma, sReg);
            var sOpMulLong2Addr = MakeRule("mul-long/2addr", sReg, comma, sReg);
            var sOpDivLong2Addr = MakeRule("div-long/2addr", sReg, comma, sReg);
            var sOpRemLong2Addr = MakeRule("rem-long/2addr", sReg, comma, sReg);
            var sOpAndLong2Addr = MakeRule("and-long/2addr", sReg, comma, sReg);
            var sOpOrLong2Addr = MakeRule("or-long/2addr", sReg, comma, sReg);
            var sOpXorLong2Addr = MakeRule("xor-long/2addr", sReg, comma, sReg);
            var sOpShlLong2Addr = MakeRule("shl-long/2addr", sReg, comma, sReg);
            var sOpShrLong2Addr = MakeRule("shr-long/2addr", sReg, comma, sReg);
            var sOpUshrLong2Addr = MakeRule("ushr-long/2addr", sReg, comma, sReg);
            var sOpAddFloat2Addr = MakeRule("add-float/2addr", sReg, comma, sReg);
            var sOpSubFloat2Addr = MakeRule("sub-float/2addr", sReg, comma, sReg);
            var sOpMulFloat2Addr = MakeRule("mul-float/2addr", sReg, comma, sReg);
            var sOpDivFloat2Addr = MakeRule("div-float/2addr", sReg, comma, sReg);
            var sOpRemFloat2Addr = MakeRule("rem-float/2addr", sReg, comma, sReg);
            var sOpAddDouble2Addr = MakeRule("add-double/2addr", sReg, comma, sReg);
            var sOpSubDouble2Addr = MakeRule("sub-double/2addr", sReg, comma, sReg);
            var sOpMulDouble2Addr = MakeRule("mul-double/2addr", sReg, comma, sReg);
            var sOpDivDouble2Addr = MakeRule("div-double/2addr", sReg, comma, sReg);
            var sOpRemDouble2Addr = MakeRule("rem-double/2addr", sReg, comma, sReg);
            var sOpAddIntLit16 = MakeRule("add-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpRsubInt = MakeRule("rsub-int", sReg, comma, sReg, comma, sHexNumber);
            var sOpSubIntLit16 = MakeRule("sub-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpMulIntLit16 = MakeRule("mul-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpDivIntLit16 = MakeRule("div-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpRemIntLit16 = MakeRule("rem-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpAndIntLit16 = MakeRule("and-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpOrIntLit16 = MakeRule("or-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpXorIntLit16 = MakeRule("xor-int/lit16", sReg, comma, sReg, comma, sHexNumber);
            var sOpAddIntLit8 = MakeRule("add-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpRsubIntLit8 = MakeRule("rsub-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpSubIntLit8 = MakeRule("sub-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpMulIntLit8 = MakeRule("mul-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpDivIntLit8 = MakeRule("div-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpRemIntLit8 = MakeRule("rem-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpAndIntLit8 = MakeRule("and-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpOrIntLit8 = MakeRule("or-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpXorIntLit8 = MakeRule("xor-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpShlIntLit8 = MakeRule("shl-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpShrIntLit8 = MakeRule("shr-int/lit8", sReg, comma, sReg, comma, sHexNumber);
            var sOpUshrIntLit8 = MakeRule("ushr-int/lit8", sReg, comma, sReg, comma, sHexNumber);

            #endregion

            #region Misc Ex Rules

            var sOpExecuteInline = MakeRule("execute-inline");
            var sOpInvokeDirectEmpty = MakeRule("invoke-direct-empty");
            var sOpIGetQuick = MakeRule("iget-quick");
            var sOpIGetWideQuick = MakeRule("iget-wide-quick");
            var sOpIGetObjectQuick = MakeRule("iget-object-quick");
            var sOpIPutQuick = MakeRule("iput-quick");
            var sOpIPutWideQuick = MakeRule("iput-wide-quick");
            var sOpIPutObjectQuick = MakeRule("iput-object-quick");
            var sOpInvokeVirtualQuick = MakeRule("invoke-virtual-quick");
            var sOpInvokeVirtualQuickRange = MakeRule("invoke-virtual-quick/range");
            var sOpInvokeSuperQuick = MakeRule("invoke-super-quick");
            var sOpInvokeSuperQuickRange = MakeRule("invoke-super-quick/range");

            #endregion

            #region Field Rules

            #region Declaration

            var sClass = Keyword(".class");
            var sSuper = Keyword(".super");
            var sImplements = Keyword(".implements");
            var sAnnotation = Keyword(".annotation");
            var sField = Keyword(".field");
            var sMethod = Keyword(".method");
            var sLocals = Keyword(".locals");
            var sEnd = Keyword(".end");
            var sSource = Keyword(".source");
            var sPrologue = Keyword(".prologue");
            var sLineKw = Keyword(".line");

            var sOpClass = new NonTerminal("class");
            var sOpSuper = new NonTerminal("super");
            var sOpImplements = new NonTerminal("implements");
            var sOpAnnotation = new NonTerminal("annotation");
            var sOpField = new NonTerminal("field");
            var sOpMethod = new NonTerminal("method");
            var sOpLocals = new NonTerminal("locals");

            #endregion

            sOpClass.Rule = sClass + sOpAccessor + sOpClassValueType;
            sOpSuper.Rule = sSuper + sOpClassValueType;
            sOpImplements.Rule = sImplements + sOpClassValueType;

            var setValue = new NonTerminal("setvalue");
            var setAnnotationList = new NonTerminal("setAnnotationlist");
            var setMultiValue = new NonTerminal("setMultiValue");
            var setMultiValueList = new NonTerminal("setMultiValueList");
            var setStaticValue = setMultiValueList | sOpClassValueType | sHexNumber | sNumber | sString | Keyword("null");
            setMultiValue.Rule = MakePlusRule(setMultiValue, comma, sOpClassValueType);
            setMultiValueList.Rule = "{" + setMultiValue + "}";
            setValue.Rule = sIdent + "=" + setStaticValue;
            setAnnotationList.Rule = MakePlusRule(setAnnotationList, setValue);
            sOpAnnotation.Rule = sAnnotation + Keyword("system") + sOpValueType + setAnnotationList + sEnd + "annotation";

            var setFieldValue = "=" + setStaticValue;
            sOpField.Rule = sField + sOpAccessor.Q() + termValueField + setFieldValue.Q();

            var sMethodLines = new NonTerminal("MethodLines");
            sMethodLines.Rule = MakePlusRule(sMethodLines, sLine);
            sOpMethod.Rule = sMethod + sOpAccessor.Q() + Keyword("native") + sMethodSignature + sEnd + "method" |
               sMethod + sOpAccessor.Q() + ToTerm("constructor").Q() + sMethodSignature + sMethodLines + sEnd + "method";


            sOpLocals.Rule = sLocals + sNumber;

            /*
                :array_0
                .array-data 4
                    0x7f02003d
                    0x7f02003c
                    0x7f020042
                    0x7f02003b
                .end array-data
             */
            var sArrayData = Keyword(".array-data");
            var sOpArrayData = new NonTerminal("array-data");
            var sArrayList = new NonTerminal("arrayList");
            sArrayList.Rule = MakePlusRule(sArrayList, sHexNumber + sComment.Q());
            sOpArrayData.Rule = sArrayData + sNumber + sArrayList + sEnd + "array-data";


            /*
                 .sparse-switch
        0x7f0c00a3 -> :sswitch_1
        0x7f0c00af -> :sswitch_18
        0x7f0c00b3 -> :sswitch_18
        0x7f0c00b7 -> :sswitch_18
    .end sparse-switch             
             */
            var sOpSparseSwitchTable = new NonTerminal("sparse-switch-table");
            var sSparseSwitchList = new NonTerminal("sparseList");
            var sSparseEntry = new NonTerminal("sparseEntry") { Rule = sHexNumber + "->" + sLocTarget };
            sSparseSwitchList.Rule = MakePlusRule(sSparseSwitchList, sSparseEntry);
            sOpSparseSwitchTable.Rule = Keyword(".sparse-switch") + sSparseSwitchList + sEnd + "sparse-switch";

            /*    .packed-switch 0x0
        :pswitch_0
        :pswitch_1
        :pswitch_2
    .end packed-switch*/

            var sOpPackedSwitchTable = new NonTerminal("packed-switch-table");
            var sPackedSwitchList = new NonTerminal("packedList");
            //var sPackedEntry = new NonTerminal("packedEntry");
            //sPackedEntry.Rule =  sLocTarget;
            sPackedSwitchList.Rule = MakePlusRule(sPackedSwitchList, sLocTarget);
            sOpPackedSwitchTable.Rule = Keyword(".packed-switch") + sHexNumber + sPackedSwitchList + sEnd + "packed-switch";

            var sOpSource = new NonTerminal("source") { Rule = sSource + sString };
            var sOpPrologue = new NonTerminal("prologue") { Rule = sPrologue };
            var sOpLineKw = new NonTerminal("line") { Rule = sLineKw + sNumber };


            //    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0
            var sCatch = Keyword(".catch");
            var sOpCatch = new NonTerminal("catch")
            {
                Rule = sCatch + sOpClassValueType + "{" + sLocTarget + ".." + sLocTarget + "}" + sLocTarget
            };

            #endregion

            sLine.Rule = sComment | sOpNop |
                sOpMove | sOpMoveFrom16 | sOpMove16 | sOpMoveWide | sOpMoveWideFrom16 | sOpMoveWide16 | sOpMoveObject | sOpMoveObjectFrom16 | sOpMoveObject16 | sOpMoveResult | sOpMoveResultWide | sOpMoveResultObject | sOpMoveException |
                sOpReturnVoid | sOpReturn | sOpReturnWide | sOpReturnObject |
                sOpConst4 | sOpConst16 | sOpConst | sOpConstHigh16 | sOpConstWide16 | sOpConstWide32 | sOpConstWide | sOpConstWideHigh16 | sOpConstString | sOpConstStringJumbo | sOpConstClass |
                sOpMonitorEnter | sOpMonitorExit | sOpCheckCast | sOpInstanceOf | sOpArrayLength | sOpNewInstance | sOpNewArray | sOpFilledNewArray | sOpFilledNewArrayRange | sOpFillArrayData | sOpThrow | sOpGoto | sOpGoto16 | sOpGoto32 |
                sOpPackedSwitch | sOpSparseSwitch | sOpCmplFloat | sOpCmpgFloat | sOpCmplDouble | sOpCmpgDouble | sOpCmpLong |
                sOpIfEq | sOpIfNe | sOpIfLt | sOpIfGe | sOpIfGt | sOpIfLe | sOpIfEqz | sOpIfNez | sOpIfLtz | sOpIfGez | sOpIfGtz | sOpIfLez |
                sOpIGet | sOpIGetWide | sOpIGetObject | sOpIGetBoolean | sOpIGetByte | sOpIGetChar | sOpIGetShort | sOpIPut | sOpIPutWide | sOpIPutObject | sOpIPutBoolean | sOpIPutByte | sOpIPutChar | sOpIPutShort |
                sOpSGet | sOpSGetWide | sOpSGetObject | sOpSGetBoolean | sOpSGetByte | sOpSGetChar | sOpSGetShort | sOpSPut | sOpSPutWide | sOpSPutObject | sOpSPutBoolean | sOpSPutByte | sOpSPutChar | sOpSPutShort |
                sOpAGet | sOpAGetWide | sOpAGetObject | sOpAGetBoolean | sOpAGetByte | sOpAGetChar | sOpAGetShort | sOpAPut | sOpAPutWide | sOpAPutObject | sOpAPutBoolean | sOpAPutByte | sOpAPutChar | sOpAPutShort |
                sOpNegInt | sOpNotInt | sOpNegLong | sOpNotLong | sOpNegFloat | sOpNegDouble | sOpIntToLong | sOpIntToFloat | sOpIntToDouble | sOpLongToInt | sOpLongToFloat | sOpLongToDouble | sOpFloatToInt | sOpFloatToLong | sOpFloatToDouble | sOpDoubleToInt | sOpDoubleToLong | sOpDoubleToFloat | sOpIntToByte | sOpIntToChar | sOpIntToShort |
                sOpAddInt | sOpSubInt | sOpMulInt | sOpDivInt | sOpRemInt | sOpAndInt | sOpOrInt | sOpXorInt | sOpShlInt | sOpShrInt | sOpUshrInt |
                sOpAddLong | sOpSubLong | sOpMulLong | sOpDivLong | sOpRemLong | sOpAndLong | sOpOrLong | sOpXorLong | sOpShlLong | sOpShrLong | sOpUshrLong |
                sOpAddFloat | sOpSubFloat | sOpMulFloat | sOpDivFloat | sOpRemFloat | sOpAddDouble | sOpSubDouble | sOpMulDouble | sOpDivDouble | sOpRemDouble |
                sOpAddInt2Addr | sOpSubInt2Addr | sOpMulInt2Addr | sOpDivInt2Addr | sOpRemInt2Addr | sOpAndInt2Addr | sOpOrInt2Addr | sOpXorInt2Addr | sOpShlInt2Addr | sOpShrInt2Addr | sOpUshrInt2Addr | sOpAddLong2Addr | sOpSubLong2Addr | sOpMulLong2Addr | sOpDivLong2Addr | sOpRemLong2Addr | sOpAndLong2Addr | sOpOrLong2Addr | sOpXorLong2Addr | sOpShlLong2Addr | sOpShrLong2Addr | sOpUshrLong2Addr | sOpAddFloat2Addr | sOpSubFloat2Addr | sOpMulFloat2Addr | sOpDivFloat2Addr | sOpRemFloat2Addr | sOpAddDouble2Addr | sOpSubDouble2Addr | sOpMulDouble2Addr | sOpDivDouble2Addr | sOpRemDouble2Addr | sOpAddIntLit16 | sOpRsubInt | sOpSubIntLit16 | sOpMulIntLit16 | sOpDivIntLit16 | sOpRemIntLit16 | sOpAndIntLit16 | sOpOrIntLit16 | sOpXorIntLit16 | sOpAddIntLit8 | sOpRsubIntLit8 | sOpSubIntLit8 | sOpMulIntLit8 | sOpDivIntLit8 | sOpRemIntLit8 | sOpAndIntLit8 | sOpOrIntLit8 | sOpXorIntLit8 | sOpShlIntLit8 | sOpShrIntLit8 | sOpUshrIntLit8 |
                sOpInvokeVirtual | sOpInvokeSuper | sOpInvokeDirect | sOpInvokeStatic | sOpInvokeInterface | sOpInvokeVirtualRange | sOpInvokeSuperRange | sOpInvokeDirectRange | sOpInvokeStaticRange | sOpInvokeInterfaceRange |
                sOpExecuteInline | sOpInvokeDirectEmpty | sOpIGetQuick | sOpIGetWideQuick | sOpIGetObjectQuick | sOpIPutQuick | sOpIPutWideQuick | sOpIPutObjectQuick | sOpInvokeVirtualQuick | sOpInvokeVirtualQuickRange | sOpInvokeSuperQuick | sOpInvokeSuperQuickRange |
                sOpClass | sOpSuper | sOpImplements | sOpAnnotation | sOpField | sOpMethod | sOpLocals |
                sTargetLocation | sOpArrayData | sOpCatch | sOpSparseSwitchTable | sOpPackedSwitchTable | sOpSource | sOpPrologue | sOpLineKw;

            sLines.Rule = MakeStarRule(sLines, sLine);
            Root = sLines;

            //Set grammar root
            MarkPunctuation("{", "}", "[", "]", ":", ",");
            MarkTransient(sLine);

        }


        public KeyTerm Keyword(string keyword, string name = "")
        {
            var term = name == "" ? ToTerm(keyword) : ToTerm(keyword, name);
            // term.SetOption(TermOptions.IsKeyword, true);
            // term.SetOption(TermOptions.IsReservedWord, true);

            MarkReservedWords(keyword);
            term.EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);

            return term;
        }

        private NonTerminal MakeRule(string termName, params BnfExpression[] format)
        {
            var temp = new NonTerminal(termName) { Rule = Keyword(termName) };
            foreach (var t in format)
                temp.Rule += t;

            return temp;
        }

    }
}
