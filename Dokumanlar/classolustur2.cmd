SET root=%cd%\UBL-TR1.2.1_Paketi\UBLTR_1.2.1_Paketi
xsd /c %root%\xsdrt\maindoc\UBL-Invoice-2.1.xsd %root%\xsdrt\common\UBL-CommonExtensionComponents-2.1.xsd %root%\xsdrt\common\UBL-CommonBasicComponents-2.1.xsd %root%\xsdrt\common\UBL-UnqualifiedDataTypes-2.1.xsd %root%\xsdrt\common\UBL-CoreComponentParameters-2.1.xsd %root%\xsdrt\common\CCTS_CCT_SchemaModule-2.1.xsd %root%\xsdrt\common\UBL-CommonAggregateComponents-2.1.xsd

PAUSE