
## Headbox tool is designed to transfer blendshapes and facial expressions between 
## the Microsoft Rocketbox avatars
## Read more: https://github.com/openVRlab/Headbox
## Creator Matias Volonte <volontematias@gmail.com>
## Northeastern University & Clemenson University

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
        #self.getGender = ''
        
        # variable created by the getGender variables
        self.gender = ''
              
        #list of json files that will be displayed in the tool
        self.jfiles = []
        self.expression_names = []
        
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
        
        self.getDeltas = ''

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
    
        mc.rowLayout(nc=4,cal=(4,'center'))


        mc.setParent('..')
   
        mc.separator(h=20)
        mc.text("Facial expression name")
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
             
    #method for selecting the root directory where the FBXs will be located and
    #based on this directory, where the FBX outputs will be exported
    def txtPathBrowse(self,*args):   
        directory = mc.fileDialog2(caption="Set working directory", dialogStyle=1, fileMode=3 )
        mc.textField(self.front, edit=1, it=directory[0])      
        self.setupFile()
        
        
    def setupFile(self,*args):
        # Select an object if and only if it exists.
        # Print a warning if it does not exist.
        
        self.path = mc.textField(self.front, q=1, text=1)
               
        #self.jsons  = self.path + self.gender
        self.dir = self.path + '/Input/'
        baseAgent = self.dir+"Female_Adult_01.fbx"
        
        if mc.objExists('f001_hipoly_81_bones_opacity_ncl1_1'):
            print("Female mesh is in the scene")
        else:
            mc.file(baseAgent, i=True)
            self.setupNeutralExpression()
    
    #method that refreshes the list of poses from the saved jsons files
    def refreshList(self,*args):

        self.path = mc.textField(self.front, q=1, text=1)
               
        #self.jsons  = self.path + self.gender
        self.dir = self.path + '/SavedFacialExpressions/'
        
        mc.textScrollList(self.s, e=True, removeAll=True)
        
        del self.jfiles[:]
        del self.expression_names[:]   
        del self.jsonsFiles[:]          
        
        self.jsonsFiles = glob.glob(self.dir + '/*.json')           
             
        #load the json files again 
        for filename in self.jsonsFiles:
            if filename.endswith(".json"):
                self.jfiles.append(filename)     
        
        fileExtension = ".json"
        
        for i in self.jfiles:
            self.expression_names.append(os.path.splitext(os.path.basename(i))[0])
       
        for obj in self.expression_names:
            mc.textScrollList(self.s, e=True, append=obj)

    #method for saving a pose
    def savePose(self,*args): 
        self.path= mc.textField(self.front, q=1, text=1)

        self.dir = self.path +'/SavedFacialExpressions/'
               
        if not os.path.exists(self.dir):
            os.makedirs(self.dir)  

        self.name = mc.textField(self.nameInput, query=True, text=True)

        poseDict = OrderedDict()
        neutralPoseDict = OrderedDict()

        jnts = pm.ls(type="joint")
        jnts = sorted(jnts)

        if self.name =="":
            mc.confirmDialog( title='Confirm', message='Facial expression name is empty', button=['Ok'], defaultButton='Ok')
        else:         
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
                print("Error saving joint data")  
         
    def setupNeutralExpression(self,*args):
            self.path= mc.textField(self.front, q=1, text=1)          

            self.name = '_Neutral.json'  

            neutralFileExists = self.path + "/SavedFacialExpressions/"+self.name
            
            poseDict = OrderedDict()
            neutralPoseDict = OrderedDict()

            jnts = pm.ls(type="joint")
            jnts = sorted(jnts)
            
            if os.path.isfile(neutralFileExists):
                print('The neutral facial expression exists')
            else:
                print('The neutral facial expression is created')

                for jnt in jnts:
                    t = list(jnt.translate.get())
                    r = list(jnt.rotate.get())
                    s = list(jnt.scale.get())
                    neutralPoseDict[jnt.nodeName()]={'t':t, 'r':r, 's':s}    
                try:
                    #neutralPoseFilePath = neutralFileExists
                    with open(neutralFileExists, 'w')as p:
                        json.dump(neutralPoseDict, p, indent=4)
                except:
                    print("Error saving joint data")   
    
    
    
    def printItem(self, *args):

        self.path= mc.textField(self.front, q=1, text=1)

        self.dir = self.path +'/SavedFacialExpressions/'

        item = mc.textScrollList( self.s, q=True, sii=True )

        index = int(item[0])

        index2 = index -1
        
        #self.gender = self.crowdTarget()
        
        try:  
           poseFilePath = self.dir 
        except:
            print "pose file path error"
        posePath = Path(poseFilePath) 
        poseJsons = [p for p in posePath.glob("*.json")]
        
        poseData = json.load(open(poseJsons[index2]))
        for j,v in poseData.items():
            for c,vals in v.items():
                j = pm.PyNode(j)
                j.attr(c).set(vals)  
                      
    #method for transferring the joints position from the jsons to all the characters
    def massExpressions(self, *args):
        self.calculateDeltas()
        self.transfer()
        
    def transfer(self):   
        mc.file(f=True, new=True)    
    
        #gender path
        self.path = mc.textField(self.front, q=1, text=1)   
        
        self.fbxpath = self.path + "/RocketboxAgents/" 
        
        self.fbxs = glob.glob(self.path +'/RocketboxAgents/*.fbx')
        
        self.resultDeltaPath = self.path + "/DeltaValues/"
         
        #list of json files 
        self.jasonFiles = os.listdir(self.path + "/SavedFacialExpressions/")
    
        for i in self.jasonFiles:
            if i.endswith(".json"):
                self.jOutput.append(i)  
                
        files = os.listdir(self.fbxpath)
        for filename in files:
            if filename.endswith(".fbx"):
                print(filename)
                self.fOutput.append(filename)  

        #AGENTS LEVEL. This collects all agents.
        for s in range(len(self.fbxs)):
            mc.file(self.fbxs[s], i=True, mergeNamespacesOnClash=True, namespace=':');
            if mc.ls(type='joint')[0][0:5]== "Bip02":
                self.renameJnts("Bip02","Bip01")              
            
            
            self.name = []
            self.neutral_pose_dict = OrderedDict()
            self.target_pose_dict= OrderedDict()
            self.resultDictionary = OrderedDict()
        
            self.blendshapes [:] = []  
            self.outputFile = self.path + "/Output/" +self.fOutput[s]
            #selects all the joints
            self.joints = mc.ls(type='joint')
            mc.select(self.joints,hi=1)     

            #select mesh
            self.name = mc.ls(type='mesh', dag=1, ni=1)

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
  
            for i in range(0,len(self.jOutput)):   

                f = open(self.resultDeltaPath+self.jOutput[i])
                self.childVals = json.load(f) 
                for key in self.neutral_pose_dict.keys():
                    if key in self.neutral_pose_dict:
                        newValue = self.childVals[key] + self.neutral_pose_dict[key] 
                        nameVal = key[5:]
                        name2 = self.joints[0] + nameVal
                        mc.setAttr(name2, newValue)                                 
                self.mesh = mc.ls(type='mesh', dag=1, ni=1)
                mc.select(self.mesh[0]) 
                #clean the json namespace     
                self.blendshapes.append(self.jOutput[i][:-5])
                mc.duplicate( name = self.jOutput[i][:-5] )            
                mc.select(clear=True)   
                                         
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
        self.resultDeltaPath = self.path + "/DeltaValues/"

        if not os.path.exists(self.resultDeltaPath):
            os.makedirs(self.resultDeltaPath)  

        #dictionaries
        self.neutral_pose_dict = OrderedDict()
        self.target_pose_dict= OrderedDict()
        self.resultDictionary = OrderedDict()

        #self.gender = self.crowdTarget()  
                   
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
            for j,v in self.poseData.items():
                for c,vals in v.items():
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
                       
  
    def renameJnts(self, oldName, newName):
        newJoints = mc.ls(type='joint')
        mc.select(newJoints,hi=1)  
           
        sel=pm.ls(selection=True)

        for each in sel:
            name=each.nodeName().replace(oldName, newName)
            each.rename(name)
            
#class instatiation           
hb = HeadBox()
#executing the GUI method
hb.buildUI()