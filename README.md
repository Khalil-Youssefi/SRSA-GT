# SRSA-GT Source Code
The present repository includes the source code of our proposed SRSA-GT algorithm (https://doi.org/10.1016/j.asoc.2022.108873). The code is written in C# language, and Unity3D is used as the development platform.

# Note:
The SRSA algorithm was separately published in our former research paper https://doi.org/10.1016/j.eswa.2021.114907.


## Ready-to-use Version

To run the ready-to-use version of the simulator without compilation or building, please refer to other our public repository: https://github.com/Khalil-Youssefi/SRSA-GT-Simulator

Please note that this simulator is built for Microsoft Windows.


## Code:

The main code of the algorithm, which is based on Algorithm 1 (Section 4.2. The Proposed Method of the research paper), is available in the file "Scripts\SRSA_GT.cs". To check on this code you need none of the below-mentioned requirements.


## Build Requirements:

- Unity Editor version 2021.1.23f1.310 Personal or later

- Microsoft Visual Studio Community 2019 Version 16.11.3  or later


## How to use:

In the Unity, in the Scenes folder, you can open and run all of the designed scenes, including S, U, H, C, and C2. There is also another scene Menu, which serves as an interface to edit the configuration file "SRSAv2_GT.cfg". This file includes the configuration of the SRSA-GT algorithm parameters. To read a full description regarding these parameters, please refer to Section (5.2. Experimental Setup) of the research paper.


## Output:

In the configuration file, you can also alter the Experiment Name. To apply the changes, please do not forget to press the "Save CFG" button.

Setting a unique Experiment Name will result in new CSV files with the results of the simulation. Please note that running multiple simulations with the same Name, Scene and Settings will be stored together in CSV files under the Experiment Name.


Lines of the main generated CSV file (i.e., file ended with the word "Records") include the following data:

+-----------------------------+

Swarm Size

Scene

Experiment

Max Memory Size

Maximum Velocity

MaxTime (minuets)

Target Radius

FF_THR

C1

C2

T_RR

MPR

MD

MPP1

MPP2

Use Mem

Use MA

NR

eo

ei

Use GT

Swarm Size

Search Duration

Iterations (time steps)

Path Length

Route traveled by the Swarm

+-----------------------------+


To reset the algorithm parameters to the default parameters (i.e., parameters suggested by the authors), use the "Load Author's CFG" button.
