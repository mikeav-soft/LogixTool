using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP.EthernetIP
{
    /// <summary>
    /// Table 2-3.2 Encapsulation Commands
    /// </summary>
    public enum EncapsulatedPacketCommand : ushort
    {
        /// <summary>
        /// may be sent only using TCP.
        /// </summary>
        NOP = 0x0000,
        /// <summary>
        /// may be sent using either UDP or TCP.
        /// </summary>
        ListServices = 0x0004,
        /// <summary>
        /// may be sent using either UDP or TCP
        /// </summary>
        ListIdentity = 0x0063,
        /// <summary>
        /// optional (may be sent using either UDP or TCP)
        /// </summary>
        ListInterfaces = 0x0064,
        /// <summary>
        /// (may be sent only using TCP)
        /// </summary>
        RegisterSession = 0x0065,
        /// <summary>
        /// (may be sent only using TCP)
        /// </summary>
        UnRegisterSession = 0x0066,
        /// <summary>
        /// (may be sent only using TCP)
        /// </summary>
        SendRRData = 0x006F,
        /// <summary>
        /// (may be sent only using TCP)
        /// </summary>
        SendUnitData = 0x0070,
        /// <summary>
        /// optional (may be sent only using TCP)
        /// </summary>
        IndicateStatus = 0x0072,
        /// <summary>
        /// optional (may be sent only using TCP)
        /// </summary>
        Cancel = 0x0073
    }
    /// <summary>
    /// Table 2-3.3 Error Codes
    /// </summary>
    public enum EncapsulatedPacketStatus : uint
    {
        Success = 0x0000,
        InvalidCommand = 0x0001,
        InsufficientMemory = 0x0002,
        IncorrectData = 0x0003,
        InvalidSessionHandle = 0x0064,
        InvalidLength = 0x0065,
        UnsupportedEncapsulationProtocol = 0x0069,
        Unknown = 0xFFFF
    }
    /// <summary>
    /// 
    /// </summary>
    public enum CommonPacketItemTypeID : ushort
    {
        Address_Null = 0x0000,
        Data_ListIdentityResponse = 0x000C,
        Address_ConnectionBased = 0x00A1,
        Data_ConnectionTransportPacket = 0x00B1,
        Data_UnconnectedMessage = 0x00B2,
        Data_ListServicesResponse = 0x0100,
        Data_SocketAddressInfoOtoT = 0x0800,
        Data_SocketAddressInfoTtoO = 0x0801,
        Address_SequencedAddressIteme = 0x0802
    }
    /// <summary>
    /// Table A-3.1 Volume 1 Chapter A-3
    /// </summary>
    public enum CIPCommonServices : byte
    {
        Get_Attributes_All = 0x01,
        Set_Attributes_All_Request = 0x02,
        Get_Attribute_List = 0x03,
        Set_Attribute_List = 0x04,
        Reset = 0x05,
        Start = 0x06,
        Stop = 0x07,
        Create = 0x08,
        Delete = 0x09,
        Multiple_Service_Packet = 0x0A,
        Apply_Attributes = 0x0D,
        Get_Attribute_Single = 0x0E,
        Set_Attribute_Single = 0x10,
        Find_Next_Object_Instance = 0x11,
        Error_Response = 0x14,
        Restore = 0x15,
        Save = 0x16,
        NOP = 0x17,
        Get_Member = 0x18,
        Set_Member = 0x19,
        Insert_Member = 0x1A,
        Remove_Member = 0x1B,
        GroupSync = 0x1C,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum CIPDataAccessServices : byte
    {
        MultiRequest = 0x03,
        GetAttributesList = 0x0A,
        ExecutePCCC = 0x4B,
        CIPReadData = 0x4C,
        CIPWriteData = 0x4D,
        ReadModifyWrite = 0x4E,
        CIPReadDataFragmented = 0x52,
        CIPWriteDataFragmented = 0x53,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ConnectionType : byte
    {
        Null = 0,
        Multicast = 1,
        Point_to_Point = 2
    }
    /// <summary>
    /// 
    /// </summary>
    public enum Priority : byte
    {
        Low = 0,
        High = 1,
        Scheduled = 2,
        Urgent = 3
    }
    /// <summary>
    /// 
    /// </summary>
    public enum RealTimeFormat : byte
    {
        Modeless = 0,
        ZeroLength = 1,
        Heartbeat = 2,
        Header32Bit = 3
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ProductionTrigger : byte
    {
        Cyclic = 0,
        CoS = 1,
        Application = 2
    }
    /// <summary>
    /// 
    /// </summary>
    public enum TransportClass : byte
    {
        Class0 = 0,
        Class1 = 1,
        Class2 = 2,
        Class3 = 3
    }

    /// <summary>
    /// Определение заголовков сегментов пути маршрутизации EPATH.
    /// </summary>
    public enum EPathSegmentHeader : byte
    {
        #region [ PORT SEGMENT ]
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 0.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_0 = 0,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 1 (Зарезервировано Backplane).
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Backplane = 1,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 2.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Network = 2,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 3.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_3 = 3,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 4.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_4 = 4,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 5.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_5 = 5,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 6.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_6 = 6,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 7.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_7 = 7,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 8.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_8 = 8,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 9.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_9 = 9,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 10.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_10 = 10,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 11.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_11 = 11,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 12.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_12 = 12,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 13.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_13 = 13,
        /// <summary>
        /// Сегмент Порта. 
        /// Port Number = 14.
        /// За заголовком следует: 
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_14 = 14,
        /// <summary>
        /// Сегмент Порта. 
        /// За заголовком следует: 
        /// + [2 byte: "Port Number"]
        /// + [1 byte: "Link Address"]
        /// </summary>
        Port_Identifier_16Bit = 15,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 0.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_0Byte = 16,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 1.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_1 = 17,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 2.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_2 = 18,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 3.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_3 = 19,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 4.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_4 = 20,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 5.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_5 = 21,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 6.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_6 = 22,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 7.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_7 = 23,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 8.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_8 = 24,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 9.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_9 = 25,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 10.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_10 = 26,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 11.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_11 = 27,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 12.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_12 = 28,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 13.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_13 = 29,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 14.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_14 = 30,
        /// <summary>
        /// Сегмент Порта.
        /// Port Number = 15.
        /// За заголовком следует: 
        /// + [2 byte: "Link Address Size" = x]
        /// + [x byte: "Link Address"]
        /// + [1 byte: "Pad Byte = 0x00"]
        /// </summary>
        Port_Extented_Identifier_16Bit = 31,
        #endregion

        #region [ LOCAL SEGMENT ]
        /// <summary>
        /// Локальный Сегмент (ClassID). 
        /// За заголовком следует: 
        /// + [1 byte: "Value"]
        /// </summary>
        Local_ClassID = 32,
        /// <summary>
        /// Локальный Сегмент (ClassID). 
        /// За заголовком следует: 
        /// + [2 byte: "Value"]
        /// </summary>
        Local_ClassID_16bit = 33,
        /// <summary>
        /// Локальный Сегмент (ClassID). 
        /// За заголовком следует: 
        /// + [4 byte: "Value"]
        /// </summary>
        Local_ClassID_32bit = 34,
        Reserved_35 = 35,
        /// <summary>
        /// Локальный Сегмент (InstanceID). 
        /// За заголовком следует: 
        /// + [1 byte: "Value"]
        /// </summary>
        Local_InstanceID = 36,
        /// <summary>
        /// Локальный Сегмент (InstanceID). 
        /// За заголовком следует: 
        /// + [2 byte: "Value"]
        /// </summary>
        Local_InstanceID_16bit = 37,
        /// <summary>
        /// Локальный Сегмент (InstanceID). 
        /// За заголовком следует: 
        /// + [4 byte: "Value"]
        /// </summary>
        Local_InstanceID_32bit = 38,
        Reserved_39 = 39,
        /// <summary>
        /// Локальный Сегмент (MemberID). 
        /// Используется для индексации массива.
        /// За заголовком следует: 
        /// + [1 byte: "Value"]
        /// </summary>
        Local_MemberID = 40,
        /// <summary>
        /// Локальный Сегмент (MemberID).
        /// Используется для индексации массива.
        /// За заголовком следует: 
        /// + [2 byte: "Value"]
        /// </summary>
        Local_MemberID_16bit = 41,
        /// <summary>
        /// Локальный Сегмент (MemberID).
        /// Используется для индексации массива.
        /// За заголовком следует: 
        /// + [4 byte: "Value"]
        /// </summary>
        Local_MemberID_32bit = 42,
        Reserved_43 = 43,
        /// <summary>
        /// Локальный Сегмент (ConnectionPoint). 
        /// За заголовком следует: 
        /// + [1 byte: "Value"]
        /// </summary>
        Local_ConnectionPoint = 44,
        /// <summary>
        /// Локальный Сегмент (ConnectionPoint). 
        /// За заголовком следует: 
        /// + [2 byte: "Value"]
        /// </summary>
        Local_ConnectionPoint_16bit = 45,
        /// <summary>
        /// Локальный Сегмент (ConnectionPoint). 
        /// За заголовком следует: 
        /// + [4 byte: "Value"]
        /// </summary>
        Local_ConnectionPoint_32bit = 46,
        Reserved_47 = 47,
        /// <summary>
        /// Локальный Сегмент (AttributeID). 
        /// За заголовком следует: 
        /// + [1 byte: "Value"]
        /// </summary>
        Local_AttributeID = 48,
        /// <summary>
        /// Локальный Сегмент (AttributeID). 
        /// За заголовком следует: 
        /// + [2 byte: "Value"]
        /// </summary>
        Local_AttributeID_16bit = 49,
        /// <summary>
        /// Локальный Сегмент (AttributeID). 
        /// За заголовком следует: 
        /// + [4 byte: "Value"]
        /// </summary>
        Local_AttributeID_32bit = 50,
        Reserved_51 = 51,
        /// <summary>
        /// Локальный Сегмент (AttributeID). 
        /// За заголовком следует: 
        /// + [1 byte: "Segment Type"]
        /// + [1 byte: "Key Format" = 0x04 ]
        /// ________________________________
        /// + [2 byte: "VendorID"]
        /// + [2 byte: "Device Type"]
        /// + [2 byte: "Product Code"]
        /// + [1 byte: "Major Revision"]
        /// + [1 byte: "Minor Revision"]
        /// </summary>
        Local_Special_ElectronicKey = 52,
        Reserved_53 = 53,
        Reserved_54 = 54,
        Reserved_55 = 55,
        /// <summary>
        /// Локальный Сегмент (ServiceID). 
        /// За заголовком следует: 
        /// + [1 byte: "Value"]
        /// </summary>
        Local_ServiceID = 56,
        /// <summary>
        /// Локальный Сегмент (ServiceID). 
        /// За заголовком следует: 
        /// + [2 byte: "Value"]
        /// </summary>
        Local_ServiceID_16bit = 57,
        /// <summary>
        /// Локальный Сегмент (ServiceID). 
        /// За заголовком следует: 
        /// + [4 byte: "Value"]
        /// </summary>
        Local_ServiceID_32bit = 58,
        Reserved_59 = 59,
        Reserved_60 = 60,
        Reserved_61 = 61,
        Reserved_62 = 62,
        Reserved_63 = 63,
        #endregion

        #region [ NETWORK SEGMENT ]
        Reserved_64 = 64,
        Network_Schedule = 65,
        Network_FixedTag = 66,
        Network_ProductionInhibitTime = 67,
        Reserved_68 = 68,
        Reserved_69 = 69,
        Reserved_70 = 70,
        Reserved_71 = 71,
        Reserved_72 = 72,
        Reserved_73 = 73,
        Reserved_74 = 74,
        Reserved_75 = 75,
        Reserved_76 = 76,
        Reserved_77 = 77,
        Reserved_78 = 78,
        Reserved_79 = 79,
        Reserved_80 = 80,
        Reserved_81 = 81,
        Reserved_82 = 82,
        Reserved_83 = 83,
        Reserved_84 = 84,
        Reserved_85 = 85,
        Reserved_86 = 86,
        Reserved_87 = 87,
        Reserved_88 = 88,
        Reserved_89 = 89,
        Reserved_90 = 90,
        Reserved_91 = 91,
        Reserved_92 = 92,
        Reserved_93 = 93,
        Reserved_94 = 94,
        Reserved_95 = 95,
        #endregion

        #region [ SYMBOLIC SEGMENT ]
        Symbolic_Extented = 96,
        Symbolic_Length_1Byte = 97,
        Symbolic_Length_2Byte = 98,
        Symbolic_Length_3Byte = 99,
        Symbolic_Length_4Byte = 100,
        Symbolic_Length_5Byte = 101,
        Symbolic_Length_6Byte = 102,
        Symbolic_Length_7Byte = 103,
        Symbolic_Length_8Byte = 104,
        Symbolic_Length_9Byte = 105,
        Symbolic_Length_10Byte = 106,
        Symbolic_Length_11Byte = 107,
        Symbolic_Length_12Byte = 108,
        Symbolic_Length_13Byte = 109,
        Symbolic_Length_14Byte = 110,
        Symbolic_Length_15Byte = 111,
        Symbolic_Length_16Byte = 112,
        Symbolic_Length_17Byte = 113,
        Symbolic_Length_18Byte = 114,
        Symbolic_Length_19Byte = 115,
        Symbolic_Length_20Byte = 116,
        Symbolic_Length_21Byte = 117,
        Symbolic_Length_22Byte = 118,
        Symbolic_Length_23Byte = 119,
        Symbolic_Length_24Byte = 120,
        Symbolic_Length_25Byte = 121,
        Symbolic_Length_26Byte = 122,
        Symbolic_Length_27Byte = 123,
        Symbolic_Length_28Byte = 124,
        Symbolic_Length_29Byte = 125,
        Symbolic_Length_30Byte = 126,
        Symbolic_Length_31Byte = 127,
        #endregion

        #region [ DATA SEGMENT ]
        Data_Simple = 128,
        Reserved_129 = 129,
        Reserved_130 = 130,
        Reserved_131 = 131,
        Reserved_132 = 132,
        Reserved_133 = 133,
        Reserved_134 = 134,
        Reserved_135 = 135,
        Reserved_136 = 136,
        Reserved_137 = 137,
        Reserved_138 = 138,
        Reserved_139 = 139,
        Reserved_140 = 140,
        Reserved_141 = 141,
        Reserved_142 = 142,
        Reserved_143 = 143,
        Reserved_144 = 144,
        /// <summary>
        /// 
        /// </summary>
        Data_Extented = 145,
        Reserved_146 = 146,
        Reserved_147 = 147,
        Reserved_148 = 148,
        Reserved_149 = 149,
        Reserved_150 = 150,
        Reserved_151 = 151,
        Reserved_152 = 152,
        Reserved_153 = 153,
        Reserved_154 = 154,
        Reserved_155 = 155,
        Reserved_156 = 156,
        Reserved_157 = 157,
        Reserved_158 = 158,
        Reserved_159 = 159,
        #endregion

        #region [ DATA TYPE SEGMENT ]
        Reserved_160 = 160,
        Reserved_161 = 161,
        Reserved_162 = 162,
        Reserved_163 = 163,
        Reserved_164 = 164,
        Reserved_165 = 165,
        Reserved_166 = 166,
        Reserved_167 = 167,
        Reserved_168 = 168,
        Reserved_169 = 169,
        Reserved_170 = 170,
        Reserved_171 = 171,
        Reserved_172 = 172,
        Reserved_173 = 173,
        Reserved_174 = 174,
        Reserved_175 = 175,
        Reserved_176 = 176,
        Reserved_177 = 177,
        Reserved_178 = 178,
        Reserved_179 = 179,
        Reserved_180 = 180,
        Reserved_181 = 181,
        Reserved_182 = 182,
        Reserved_183 = 183,
        Reserved_184 = 184,
        Reserved_185 = 185,
        Reserved_186 = 186,
        Reserved_187 = 187,
        Reserved_188 = 188,
        Reserved_189 = 189,
        Reserved_190 = 190,
        Reserved_191 = 191,
        Reserved_192 = 192,
        DataType_BOOL = 193,
        DataType_SINT = 194,
        DataType_INT = 195,
        DataType_DINT = 196,
        DataType_LINT = 197,
        Reserved_198 = 198,
        Reserved_199 = 199,
        Reserved_200 = 200,
        Reserved_201 = 201,
        DataType_REAL = 202,
        Reserved_203 = 203,
        Reserved_204 = 204,
        Reserved_205 = 205,
        Reserved_206 = 206,
        Reserved_207 = 207,
        Reserved_208 = 208,
        Reserved_209 = 209,
        Reserved_210 = 210,
        DataType_DWORD = 211,
        Reserved_212 = 212,
        Reserved_213 = 213,
        Reserved_214 = 214,
        Reserved_215 = 215,
        Reserved_216 = 216,
        Reserved_217 = 217,
        Reserved_218 = 218,
        Reserved_219 = 219,
        Reserved_220 = 220,
        Reserved_221 = 221,
        Reserved_222 = 222,
        Reserved_223 = 223,
        #endregion

        #region [ RESERVED SEGMENT ]
        Reserved_224 = 224,
        Reserved_225 = 225,
        Reserved_226 = 226,
        Reserved_227 = 227,
        Reserved_228 = 228,
        Reserved_229 = 229,
        Reserved_230 = 230,
        Reserved_231 = 231,
        Reserved_232 = 232,
        Reserved_233 = 233,
        Reserved_234 = 234,
        Reserved_235 = 235,
        Reserved_236 = 236,
        Reserved_237 = 237,
        Reserved_238 = 238,
        Reserved_239 = 239,
        Reserved_240 = 240,
        Reserved_241 = 241,
        Reserved_242 = 242,
        Reserved_243 = 243,
        Reserved_244 = 244,
        Reserved_245 = 245,
        Reserved_246 = 246,
        Reserved_247 = 247,
        Reserved_248 = 248,
        Reserved_249 = 249,
        Reserved_250 = 250,
        Reserved_251 = 251,
        Reserved_252 = 252,
        Reserved_253 = 253,
        Reserved_254 = 254,
        Reserved_255 = 255,
        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    public enum EPathToByteMethod
    {
        Complete,
        DataOnly
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ListIdentityState : byte
    {
        Nonexistent = 0,
        DeviceSelfTesting = 1,
        Standby = 2,
        Operational = 3,
        MajorRecoverableFault = 4,
        MajorUnrecoverableFault = 5,
        DefaultforGet_Attributes_All_service = 255
    }
    /// <summary>
    /// 
    /// </summary>
    public enum EIPBaseProtocol
    {
        Tcp,
        UdpPont,
        UdpBroadcast
    }
}
