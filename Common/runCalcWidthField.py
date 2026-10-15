import arcpy
arcpy.env.overwriteOutput = True


intable = arcpy.GetParameterAsText(0)
fieldName = arcpy.GetParameterAsText(1)
multiplier = float(arcpy.GetParameter(2))
exponent = float(arcpy.GetParameter(3))
bufferMultiplier = int(arcpy.GetParameter(4))
expression = "calcWidth(float(!DSContArea!), multiplier, exponent, bufferMultiplier)"

codeblock = '''
def calcWidth(area, multiplier, exponent, bufferMultiplier):
    areasqkm = area / 1E6
    streamWidth = multiplier * (areasqkm ** exponent) 
    return streamWidth * bufferMultiplier
'''

arcpy.management.CalculateField(intable, fieldName, expression, "PYTHON3", codeblock)

# The following message will be included in the message box from the calling button's OnClick routine
print("Calculation of {0} field complete.".format(fieldName))
