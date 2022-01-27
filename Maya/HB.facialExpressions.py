import maya.cmds as mc
import os
import json
import pymel.core as pm
from pymel.core import Path
from collections import OrderedDict
import glob
from functools import partial

class HeadBox():
    def __init__(self):
        #This function displays the GUI
        self.buildUI()
        
        #selected root directory obtained from the input box. This
        #directory will contain all the FBX's that the blendshapes will     
        #be transferred        
        self.path = ''

        #directory composed of the gender variable and the selected path       
        self.dir = ''
        
        #this variable is defined by the selected saved pose in the GUI
        self.item = ''  
        
        #name typed by the user for saving the pose
        self.name = ''
        
        #variable that is returned after selecting the gender crowd group
        self.getGender = ''
        
        # variable created by the getGender variables
        self.gender = ''
              
        #list of json files that will be displayed in the tool
        self.jfiles = []
        
        #variable that collects all the saved json files
        self.jsonsFiles = []
        
        #json file that will be used for creating the blendshapes 
        self.jOutput = [] 
        
        #fbx file variable
        self.fOutput = []  
        
        #list of blendshapes
        self.blendshapes = []  
        
        #list of fbx files in a directory
        self.fbxs = []

        #selection of shapes in the maya scene
        self.agent = []
        
        #stores only the character mesh and clears unicode in the string
        self.mesh = []
        
        #pose from the saved json file 
        self.poseData = []
        
        #fbx output path and name of the output fbx
        self.finalFile =[]

        #variable that will hold the selected pose item from GUI
        self.index = 0
 
        #fix index offset returned from maya 
        self.index2 = 0

    #method that creates the window    
    def buildUI(self):
    
        #checks if a window is already displayed. 
        #If it is, it closes and opens a new one (for avoiding multiple windows)
        if mc.window("HeadBox",e=True, exists=True):
            mc.deleteUI("HeadBox")
        mc.window("HeadBox", s=True, widthHeight=(400, 600))
        
        self.tabs = mc.tabLayout(innerMarginWidth=5, innerMarginHeight=5)
        self.child1 = mc.columnLayout(adj=True)
       
        mc.separator(h=20)
        mc.button(label='Select Directory',command=self.txtPathBrowse)
        mc.text("Directory")
        self.front = mc.textField()
        mc.separator(h=20)
        mc.text("Choose Crowd Population")
        mc.separator(h=20)
    
        mc.rowLayout(nc=3,cal=(3,'center'))
        mc.radioCollection()
    
        self.child = mc.radioButton(al='left', label='Child')
        self.female = mc.radioButton(al='center', label='Female')
        self.male = mc.radioButton(al='right', label='Male')
        mc.setParent('..')
   
        mc.separator(h=20)
        self.nameInput = mc.textField()
        mc.button(label='Save Facial Expression',command=self.savePose)
        mc.button(label='Refresh',command=self.refreshList)
        mc.separator(h=20)
    
        mc.paneLayout()
        self.s = mc.textScrollList()
    
        self.selection = mc.textScrollList(self.s, e=True, dcc=partial(self.printItem,self.s))
    
        mc.separator(h=20)
        mc.setParent('..')
    
        mc.button(label='Transfer Blendshapes', command=self.massExpressions)
    
        mc.separator(h=20)
    
        mc.setParent( '..' )
    
        self.child2 = mc.columnLayout(bgc=(0.75,.75,.75))
        mc.separator(h=20)   
        mc.text( label='Visit the website for a guide on how to use this tool:' )
        mc.text( label='')
        
        mc.text( l='<a href="https://docs.google.com/document/d/13J1IzCX8vjnoFGS9PP_WcRdb2f5RPHfXhOGLgBNMm7Q/edit?usp=sharing"> hey click here.</a>',hl=True, align='center' )
  
        mc.separator( style='none' )

        mc.setParent( '..' )
    
        mc.tabLayout( self.tabs, edit=True, tabLabel=((self.child1, 'Application'), (self.child2, 'Help')) )
    
        mc.showWindow()

    #function that returns the users' selected gender and group targeted to outputs the blendshapes 
    def crowdTarget(self, *args):
        if (mc.radioButton(self.child, query = True, select = True)):
            self.getGender = "/child/"

        if (mc.radioButton(self.female, query = True, select = True)):
            self.getGender = "/female/"

        if (mc.radioButton(self.male, query = True, select = True)):
            self.getGender = "/male/"
        
        return self.getGender
    
    #method that refreshes the list of poses from the saved jsons files
    def refreshList(self,*args):
        self.path = mc.textField(self.front, q=1, text=1)
        
        self.gender = self.crowdTarget()        
        
        #self.jsons  = self.path + self.gender
        self.dir = self.path + self.gender
        
        mc.textScrollList(self.s, e=True, removeAll=True)
        
        self.jsonsFiles = glob.glob(self.dir + '/*.json')
        
        #clear list of json files displayed in the GUI
        del self.jfiles[:]
         
        #load the json files again 
        for filename in self.jsonsFiles:
            if filename.endswith(".json"):
                self.jfiles.append(filename)     

        for obj in self.jfiles:
            mc.textScrollList(self.s, e=True, append=obj)

    #method for saving a pose
    def savePose(self,*args):        
        self.path= mc.textField(self.front, q=1, text=1)

        self.name = mc.textField(self.nameInput, query=True, text=True)

        if self.name =="Neutral" or self.name =="neutral":
            self.name = '_Neutral'
        
        self.gender = self.crowdTarget()
        

        if self.path=="":
            mc.confirmDialog( title='Select Root directory', message='Please select Root directory', button=['Ok'], defaultButton='Ok' )                          
            return;
        if self.gender=="":
            mc.confirmDialog( title='Select Crowd Population', message='Please select crowd population', button=['Ok'], defaultButton='Ok' )                          
            return;
        else:
            self.dir = self.path + self.gender
        
        if not os.path.exists(self.dir):
            os.makedirs(self.dir)  
                       
        jnts = pm.ls(type="joint")
        jnts = sorted(jnts)
        
        poseDict = OrderedDict()
        
        for jnt in jnts:
            t = list(jnt.translate.get())
            r = list(jnt.rotate.get())
            s = list(jnt.scale.get())
            poseDict[jnt.nodeName()]={'t':t, 'r':r, 's':s}    
        try:
            poseFilePath = self.dir + self.name +".json"
            with open(poseFilePath, 'w')as p:
                json.dump(poseDict, p, indent=4)
        except:
            print "gender"   
    
    #method for selecting the root directory where the FBXs will be located and
    #based on this directory, where the FBX outputs will be exported
    def txtPathBrowse(self,*args):   
        directory = mc.fileDialog2(caption="Set working directory", dialogStyle=1, fileMode=3 )
        mc.textField(self.front, edit=1, it=directory[0])
        
    def printItem(self, *args):

        path = mc.textField(self.front, q=1, text=1)

        item = mc.textScrollList( self.s, q=True, sii=True )

        index = int(item[0])

        index2 = index -1
        
        self.gender = self.crowdTarget()
        
        try:  
           poseFilePath = path + self.gender  
        except:
            print "pose file path error"
        posePath = Path(poseFilePath) 
        poseJsons = [p for p in posePath.glob("*.json")]
        
        poseData = json.load(open(poseJsons[index2]))
        for j,v in poseData.iteritems():
            for c,vals in v.iteritems():
                j = pm.PyNode(j)
                j.attr(c).set(vals)  
                        
    #method for transferring the joints position from the jsons to all the characters
    def massExpressions(self, *args):  
        self.calculateDeltas()
        self.transferExpressions()
        
    def transferExpressions(self):
        self.fbxs = glob.glob(self.path+'/*.fbx')
        #gender path
        self.path = mc.textField(self.front, q=1, text=1)
  
        #list of json files 
        self.jasonFiles = os.listdir(self.path + self.gender)
    
        for i in self.jasonFiles:
            if i.endswith(".json"):
                self.jOutput.append(i)  
  
       
        self.gender = self.crowdTarget()
            
        self.posePath = Path(self.path + self.gender)        
        
        self.exportDir = self.posePath + "/export/"
        #####
        self.deltaPath = self.path + self.gender

        #directory where the delta files will be saved
        self.resultDeltaPath = self.path+"/DeltaValues/"

        #save neutral pose of each character (only one per group) to apply the delta later
        self.neutralPoses = self.path+"/NeutralPose/"   


        files = os.listdir(self.path)
        for filename in files:
            if filename.endswith(".fbx"):
                self.fOutput.append(filename)  


      
        if not os.path.exists(self.neutralPoses):
            os.makedirs(self.neutralPoses)  
 

        for s in range(len(self.fbxs)):
            mc.file(self.fbxs[s], i=True, mergeNamespacesOnClash=True, namespace=':');
            self.name = []

            self.neutral_pose_dict = OrderedDict()
            self.target_pose_dict= OrderedDict()
            self.resultDictionary = OrderedDict()
        
            self.blendshapes [:] = []  
               
            self.joints = mc.ls(type='joint')
            mc.select(self.joints,hi=1)     


            self.name = mc.ls(type='mesh', dag=1, ni=1)
            
            self.outputFile = self.path + "/output/" +self.fOutput[s]

            
            for jnt in self.joints:
                    otx = mc.getAttr(jnt+'.tx')  
                    oty = mc.getAttr(jnt+'.ty')     
                    otz = mc.getAttr(jnt+'.tz')
                    
                    orx = mc.getAttr(jnt+'.rx')  
                    ory = mc.getAttr(jnt+'.ry')     
                    orz = mc.getAttr(jnt+'.rz')    
                    
                    osx = mc.getAttr(jnt+'.sx')  
                    osy = mc.getAttr(jnt+'.sy')     
                    osz = mc.getAttr(jnt+'.sz')       
                    
                    self.neutral_pose_dict [jnt+".tx"] = otx
                    self.neutral_pose_dict [jnt+".ty"] = oty    
                    self.neutral_pose_dict [jnt+".tz"] = otz                                         
                    
                    self.neutral_pose_dict [jnt+".rx"] = orx
                    self.neutral_pose_dict [jnt+".ry"] = ory    
                    self.neutral_pose_dict [jnt+".rz"] = orz        
                    
                    self.neutral_pose_dict [jnt+".sx"] = osx
                    self.neutral_pose_dict [jnt+".sy"] = osy    
                    self.neutral_pose_dict [jnt+".sz"] = osz  

            self.finale_name = self.neutralPoses + self.name[0][0:2] +".json"
 
            if(os.path.isfile(self.finale_name)==False):
                self.neutralPose = json.dumps(self.neutral_pose_dict, sort_keys=True, ensure_ascii = True, indent = 2) 
                f= open(self.finale_name, 'w')
                f.write(self.neutralPose)
                f.close    
                self.target_pose_dict.clear() 

            self.neutralFE = open(self.finale_name) 
            self.deltaVals = json.load(self.neutralFE)                                

            for i in range(0,len(self.jOutput)):   

                f = open(self.resultDeltaPath+self.jOutput[i])

                self.childVals = json.load(f) 
                for key in self.deltaVals.keys():
                    if key in self.deltaVals:
                        newValue = self.childVals[key] + self.deltaVals[key] 
                        nameVal = key[5:]
                        name2 = self.joints[0] + nameVal
                        mc.setAttr(name2, newValue)                                 
                self.mesh = mc.ls(type='mesh', dag=1, ni=1)
                mc.select(self.mesh[0]) 
                #clean the json namespace     
                self.blendshapes.append(self.jOutput[i][:-5])
                mc.duplicate( name = self.jOutput[i][:-5] )            
                mc.select(clear=True)                   
                             
                #setNeutralPose()  
                                         
            for i in self.blendshapes:
                mc.select(i, add=True) 
            mc.select(self.mesh[0], add=True) 
            mc.blendShape() 
            for i in self.blendshapes:
                try:
                    mc.delete(i)   
                except:
                    print ""                    
            mel.eval('FBXExport -f "%s" -s' % self.outputFile )                               
            mc.file(f=True, new=True)    
            
    def calculateDeltas(self):
        #gender path
        self.path = mc.textField(self.front, q=1, text=1)

        #####
        self.deltaPath = self.path + self.gender

        #directory where the delta files will be saved
        self.resultDeltaPath = self.path+"/DeltaValues/"

        if not os.path.exists(self.resultDeltaPath):
            os.makedirs(self.resultDeltaPath)  

        #dictionaries
        self.neutral_pose_dict = OrderedDict()
        self.target_pose_dict= OrderedDict()
        self.resultDictionary = OrderedDict()

        self.gender = self.crowdTarget()  
                   
        self.joints = mc.ls(type='joint')
        mc.select(self.joints,hi=1)     

        for jnt in self.joints:
                otx = mc.getAttr(jnt+'.tx')  
                oty = mc.getAttr(jnt+'.ty')     
                otz = mc.getAttr(jnt+'.tz')
                
                orx = mc.getAttr(jnt+'.rx')  
                ory = mc.getAttr(jnt+'.ry')     
                orz = mc.getAttr(jnt+'.rz')    
                
                osx = mc.getAttr(jnt+'.sx')  
                osy = mc.getAttr(jnt+'.sy')     
                osz = mc.getAttr(jnt+'.sz')       
                
                self.neutral_pose_dict [jnt+".tx"] = otx
                self.neutral_pose_dict [jnt+".ty"] = oty    
                self.neutral_pose_dict [jnt+".tz"] = otz                                         
                
                self.neutral_pose_dict [jnt+".rx"] = orx
                self.neutral_pose_dict [jnt+".ry"] = ory    
                self.neutral_pose_dict [jnt+".rz"] = orz        
                
                self.neutral_pose_dict [jnt+".sx"] = osx
                self.neutral_pose_dict [jnt+".sy"] = osy    
                self.neutral_pose_dict [jnt+".sz"] = osz  


        for i in range(len(self.jsonsFiles)):
            #print self.jsonsFiles[i][31:-5]
            self.poseData = json.load(open(self.jsonsFiles[i]))
            for j,v in self.poseData.iteritems():
                for c,vals in v.iteritems():
                    j = pm.PyNode(j)
                    j.attr(c).set(vals)  
                    
            self.joints = mc.ls(type='joint')
            mc.select(self.joints,hi=1)

            for jnt in self.joints:
                self.otx = mc.getAttr(jnt+'.tx')  
                self.oty = mc.getAttr(jnt+'.ty')     
                self.otz = mc.getAttr(jnt+'.tz')      
                
                self.trx = mc.getAttr(jnt+'.rx')  
                self.ttry = mc.getAttr(jnt+'.ry')     
                self.trz = mc.getAttr(jnt+'.rz')    
                
                self.tsx = mc.getAttr(jnt+'.sx')  
                self.tsy = mc.getAttr(jnt+'.sy')     
                self.tsz = mc.getAttr(jnt+'.sz')       
                
                self.target_pose_dict [jnt+".tx"] = self.otx
                self.target_pose_dict [jnt+".ty"] = self.oty    
                self.target_pose_dict [jnt+".tz"] = self.otz                                         
                
                self.target_pose_dict [jnt+".rx"] = self.trx
                self.target_pose_dict [jnt+".ry"] = self.ttry    
                self.target_pose_dict [jnt+".rz"] = self.trz        
                
                self.target_pose_dict [jnt+".sx"] = self.tsx
                self.target_pose_dict [jnt+".sy"] = self.tsy    
                self.target_pose_dict [jnt+".sz"] = self.tsz  
                
                
            for key in self.neutral_pose_dict.keys():
                if key in self.neutral_pose_dict:
                    if key in self.target_pose_dict:
                        self.newValue = self.target_pose_dict[key] - self.neutral_pose_dict[key] 
                        self.resultDictionary[key] = self.newValue
                    else:
                        print(key, 'is not in dictionary2')
                else:
                    print(key, 'is not in dictionary1')            
                           
            self.deltaExpression = json.dumps(self.resultDictionary, sort_keys=True, ensure_ascii = True, indent = 2)
            f= open(self.resultDeltaPath + os.path.split(self.jsonsFiles[i])[-1], 'w')
            f.write(self.deltaExpression)
            f.close    
            self.target_pose_dict.clear()
  
    def replaceJntsNames(self, inputFile):  
        fin = open("Expression_delta/NeutralPose/"+inputFile+".json", "rt")
        #read file contents to string
        data = fin.read()
        #replace all occurrences of the required string
        data = data.replace('Bip02', 'Bip01')
        #close the input file
        fin.close()
        #open the input file in write mode
        fin = open("/Expression_delta/NeutralPose/"+inputFile+".json", "wt")
        #overrite the input file with the resulting data
        fin.write(data)
        #close the file
        fin.close()
        
        #self.replaceJntsNames("cf")
        #self.replaceJntsNames("cm")   
  
#class instatiation           
hb = HeadBox()
#executing the GUI method
hb.buildUI()