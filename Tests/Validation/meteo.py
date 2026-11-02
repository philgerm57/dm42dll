#-------------------------------------------------------------------------------
# Name:        module1
# Purpose:
#
# Author:      germain
#
# Created:     18/12/2023
# Copyright:   (c) germain 2023
# Licence:     <your licence>
#-------------------------------------------------------------------------------

import math


# Phase de la lune pour la DM42 (période synodique, référence 1er janvier 2001)

def jours(j, m, a):
    # nombre de jours depuis le 01/01/2001 (calendrier grégorien)
    if m < 3:
        m = m + 12
        a = a - 1
    n = 365 * a + int(a / 4) - int(a / 100) + int(a / 400)
    return n + int(306 * (m + 1) / 10) + j - 730914

def position(d, h):
    j, m, a = dmy(d)
    n = jours(j, m, a) + h / 24
    return (0.20439731 + n * 0.03386319269) % 1

def phase(pos):
    i = int(pos * 8 + 0.5) % 8
    if i == 0:
        print("Nouvelle lune")
    elif i == 1:
        print("Premier croissant")
    elif i == 2:
        print("Premier quartier")
    elif i == 3:
        print("Gibbeuse croissante")
    elif i == 4:
        print("Pleine lune")
    elif i == 5:
        print("Gibbeuse decroissante")
    elif i == 6:
        print("Dernier quartier")
    else:
        print("Dernier croissant")



"""

La pression de vapeur saturante ou tension de vapeur est la pression à laquelle
la phase gazeuse de cette substance est en équilibre avec sa phase liquide ou solide.
Elle dépend de la température. Certains utilisent le terme " pression de vapeur ",
mais celui-ci est ambigu car il évoque la pression partielle de la vapeur.

La pression de vapeur saturante est la pression partielle de la vapeur
d'un corps pur à partir de laquelle une partie du corps pur passe
sous forme liquide ou solide. On dit aussi que c'est la pression maximale de sa vapeur
" sèche " (c'est-à-dire sans phase liquide).

Quand la pression partielle de la vapeur est égale à la pression
de vapeur saturante d'une substance, les phases gazeuse,liquide ou solide sont en équilibre.

Si la pression partielle de la vapeur dépasse la pression de vapeur saturante,
il y a donc liquéfaction ou condensation. À partir d'une situation d'équilibre,
cela peut se faire en augmentant la pression partielle de vapeur (par exemple en diminuant le volume)
ou bien en diminuant la pression de vapeur saturante, c'est-à-dire en diminuant la température.

En résumé :

    psat(T) > pamb : ébullition, vaporisation
    pamb > psat(T) > pvap : évaporation
    psat(T) = pvap : équilibre
    pvap > psat(T) : liquéfaction

où psat(T) est la pression de vapeur saturante,
pvap est la pression de vapeur (pression partielle de la vapeur)
et pamb est la pression ambiante

HR% = (Pvap / Psat(T)) * 100

"""

# calcul de la pression de vapeur saturante avec temp en °C
def P1_Psat(temp):
    tempk= temp + 273.15
    psat= 23.3265 - (3802.7 / tempk)  - pow((472.68/ tempk),2)
    return math.exp(psat)

# calcul de la pression de vapeur saturante avec temp en °C
# via  NF EN ISO 13788:2012
def P2_Psat(temp):
    ftemp = float(temp)
    if ftemp>=0:
        psat = 610.5 * math.exp((17.269 * ftemp)/ (237.3+ftemp))
    else:
        psat = 610.5 * math.exp((21.875 * ftemp)/ (265.5+ftemp))
    return psat


# calcul de la pression de vapeur saturante avec temp en °C
# via la formule de rankine
def P3_Psat(temp):
    tempK = float(temp)+ 273.15
    psat = 1013.25 * math.exp(13.7 - 5120 / tempK)
    return psat





# calcul de la pression de vapeur saturante avec temp en °C
# via la formule de Wagner et Pruss
# !! resultat erroné pour 20°C devrait donné 2339.194 pa
def P4_Psat(temp):
    tempK = float(temp)+ 273.15
    v  = 1- (tempK / 647.096)
    p1 = 647.096 / tempK
    p1 = -7.85951783 * v
    p2 = 1.84408259 * math.pow(v,1.5)
    p3 = -11.7866497 * math.pow(v,3.0)
    p4 = 22.680741 * math.pow(v,3.5)
    p5 = -15.9618719 * math.pow(v,4.0)
    p6 = 1.80122502 * math.pow(v,7.5)
    lnpsat = v* (p1 + p2 + p3 + p4 + p5 + p6)
    psat = math.exp(lnpsat) * 22.064000 * math.pow(10,6)
    return psat

# calcul de la pression de vapeur saturante avec temp en °C
# via la formule de Clausius-Clapeyron pour P1 = 1 atm
# T0 ebullition au niveau de la mer = 100° soit 373°K

def P5_Psat(temp):
    tempK = float(temp)+ 273.15
    psati = (40680.0 / 8.31447) * ((1.0/373.0)  - (1.0/tempK ))
    psat = math.exp(psati) * 1013
    return psat


"""
DEWPOINT

Formula:
The following method is used to calculate dewpoint:

v = RH*0.01*6.112 * exp [(17.62*T)/(T + 243.12)],
this equation will provide the vapor pressure value (in pressure units) where T is the air

temperature in C and RH is the relative humidity. Now dewpoint, Td, can be found:
Numerator = 243.12*(ln v) – 440.1
Denominator = 19.43 – ln v

Td = Numerator/Denominator

This equation is an approximation of the Goff & Gratch equation, which is extremely complex. This equation is one recommended
 by the World Meteorological Organization for saturation of air with respect to water.

The Vantage Pro and Vantage Pro2 console uses a lookup table and it only differs from the formula in that whole degrees
 and less resolution in the table are used for code and memory space conservation.


"""

# calcul du point de rosée en fonction de la temperature en °C et de l'humidite en %
def CalPtRosee (temp,hr):
    p1 =(17.27 * temp)/ (237.7 + temp)
    pr = (237.7 * ( p1 + math.log(hr))) / (17.27 - (p1 + math.log(hr)))
    return pr

#calcul de l'humidite spécifique  en fonction de l'humidité relative en % et de la temperature °C
def HumSpecif(temp,hr):
    # calcul de Psat
    psat = P1_Psat(float(temp))
    hs= (0.622 * psat*float(hr)/100) / (101325 - psat * float(hr)/100)
    return hs

# KO
def T1_TempHumide(pres,temp,hum):
    th= (-2500.0 / 1005) * (0.622 * (253000000.0 / float(pres)) * math.exp(5420.0 / float(temp)) - float(hum) / 100 )
    return th

# Calcul du bulbe humide formule OK
def T2_TempHumide(pres,temp,hum):
    ftemp= float(temp)
    fhum = float(hum)
    Tw = ftemp * math.atan(0.152 *  math.pow(fhum +8.3136,0.5)) + math.atan(ftemp + hum) - math.atan(hum - 1.6763) + 0.00391838 * math.pow(fhum,3.0/2.0) * math.atan(0.0231 * fhum) - 4.686
    return Tw

# KO
def T3_TempHumide(pres,temp,hum):
    fhum = float(hum ) / 100.0
    ftempK = float (temp) + 273.15
    thum= (0.2831 * math.pow(fhum,0.2735)* ftempK) + (0.0003018 * math.pow(fhum,2)) + (0.01289 * fhum) - 4.0962
    return thum


# calcul windchill temperature ressentiee
def wct(temp,vitv):
    ftemp = float(temp)
    fvitv = float(vitv)
    wdc= 13.12 + 0.6215*ftemp -11.37 * math.pow(fvitv,0.16) + 0.3965*ftemp*math.pow(fvitv,0.16)
    return wdc

"""
 autre formle winfchill DAVIS  V = vitesse du vent
0.0817 * (3.71V0.5 + 5.81 – 0.25V) * (T – 91.4) + 91.4
Formulas:
Older versions of software (Versions 5.0 and earlier) and firmware (Vantage firmware revisions before
 Sept. 7, 2001 and all non-VantagePro products including Echo) are based on the following formula (Siple and Passel, 1945):

0.0817 * (3.71V0.5 + 5.81 – 0.25V) * (T – 91.4) + 91.4

where V is the wind speed in mph and T is the outside air temperature in °F. Wind speeds above 55 mph are set to 55 mph.
 For wind speeds below 5 mph or temperatures above 91.4°F, the wind chill is set equal to the air temperature.

Newer product revisions (WeatherLink version 5.1 through 5.5.1 and Vantage Pro and Vantage Pro2 consoles with Sept 7, 2001
 firmware or later and Vantage Pro2 consoles with firmware before May 2005) are based on the following formula:

35.74 + 0.6215T – 35.75 * (V 0.16 ) + 0.4275T * (V 0.16 )

"""


# calcul de la temperature virtuelle de la colonne d'air fictive
# en fonction de la pression atmosphrique en hPa , temp en °C et hr en %
# retourne la temperature en °C

def T1_tvirt(pres, temp,hr):
    tk = float(temp)+ 273.15
    tv= tk /( 1- (0.3783 * float(hr)/100.0* P1_Psat(float(temp))) / (float(pres)*100))
    return tv -273.15


def T2_tvirt(pres,temp,hr):
    DP = CalPtRosee(temp,hr/100)
    pu=  7.5 * DP / (237.7+DP)
    e= 6.11 * math.pow(10,pu)
    Tv= temp / (1- 0.379 * e / pres)
    return Tv


# calcul de la masse volumique de l'air



# calcul pression partielle de vapeur d'eau
def calcE(temp):
    td= float(temp)
    vale=0.0
    if td <9.1:
        vale= 5.6402 * (-0.0916 + math.exp(0.06* td))
    else:
        vale= 18.2194 * (1.0463 + math.exp(0.06* td))
    return vale

"""

BAROMETRIC PRESSURE

What is it:
The weight of the air that makes up our atmosphere exerts a pressure on the surface of the earth. This pressure is known as atmospheric pressure.
 Generally, the more air above an area, the higher the atmospheric pressure, this, in turn, means that atmospheric pressure changes with altitude.
  For example, atmospheric pressure is greater at sea-level than on a mountaintop. To compensate for this difference and facilitate comparison between
   locations with different altitudes, atmospheric pressure is generally adjusted to the equivalent sea-level pressure. This adjusted pressure is known as barometric pressure. In reality, the Vantage Pro and Vantage Pro2 measures atmospheric pressure. When entering the location’s altitude in Setup Mode, the Vantage Pro and Vantage Pro2 calculates the necessary correction factor to consistently translate atmospheric pressure into barometric pressure.

Barometric pressure also changes with local weather conditions, making barometric pressure an extremely important and useful weather forecasting tool.
High pressure zones are generally associated with fair weather while low pressure zones are generally associated with poor weather. For forecasting purposes,
however, the absolute barometric pressure value is generally less important than the change in barometric pressure. In general, rising pressure indicates
improving weather conditions while falling pressure indicates deteriorating weather conditions.

The following section applies to Vantage Pro and Vantage Pro2 systems only:
Parameters Used: Outside Air Temperature, Outside Humidity, Elevation, Atmospheric Pressure

Formula: Simply,

PSL = PS * (R),
where PSL is sea level pressure, PS is the unadjusted reading sensed by the Davis barometer,

and R is the reduction ratio, which is determined as follows:

First, Tv (virtual temperature in the “fictitious column of air” extending down to sea-level) can be determined as follows.
The result is in degrees Rankine, which is similar to Kelvin except it uses a Fahrenheit scale divisions rather than Celsius scale divisions:

Tv = T + 460 + L + C,

where T is the average between the current outdoor temperature and the temperature 12 hours ago (in Fahrenheit) in whole degrees.
L is the typical lapse rate, or decrease in temperature with height (of the “fictitious column of air”), as calculated by:

L = 11 Z/8000,
where L is a constant value with units in °F. Z is elevation, which must be entered in feet.

The current dewpoint value and the station elevation are necessary to compute C. C is the correction for the humidity in the “fictitious column of air”.
 It is determined from a lookup table (provided in the attached table). The table consists of dewpoints in °F every 4°F and elevations

in feet every 1500 feet. Linear interpolation is performed to obtain the correct reduced pressure value. For dewpoints below –76°F, C = 0;
 for dewpoints above 92°F, a dewpoint of 92°F is assumed.

Now, Tv can be determined. From this, the following can be computed: Exponent = [Z/(122.8943111*Tv)]
Once this exponent is computed, R can be computed from the following: R = 10^[Exponent].

Thus, PSL = PS * (R) can be calculated. Pressure can be in any units (R is dimensionless) and still yield the correct value.

This procedure is designed to produce the correct reduced sea-level pressure as displayed. This requires the user to know their elevation
 to at least plus or minus 10 ft. to be accurate to every 0.1 ” HG

or plus or minus 3 feet to be accurate to every 0.1 mb/hPa.

This is a simplified version of the official U.S. version in place now. The accepted method is to use lookup tables of ratio
reduction values keyed to station temperature. These are based on station climatology. These values are unavailable for every
possible location where a Davis user may have a station, thus this approach is not suitable.

It should be noted that if a sensor’s pressure readings require adjustment, the user can adjust either the uncorrected or
the final reading to match the user’s reference, as appropriate. If the user chooses to measure uncorrected atmospheric pressure
or use another reduction method, they should set their elevation to zero. Subsequently, output data using the VantageLink
 can be read by or exported to another application and converted as desired.

The calibration of the sensor is a separate one time function performed on the unit during the manufacturing process.
It is a completely independent operation from the calculation the Vantage Pro and Vantage Pro2 console makes to display
a reading corrected to sea-level. The calibration is done to ensure the sensor reads uncorrected or raw atmospheric pressure
(not barometric pressure) properly. Any properly functioning unit will read the uncorrected atmospheric pressure within specifications.
However, limits in the displayable range of the bar value may prevent the user from setting an incorrect elevation for their location.
That is, a user at sea-level, may see a dashed reading if they set their unit to 5000′ elevation or vice-versa. So, the best way to tell if a unit is functioning properly, is:


z=(RL·Tv/g)·ln(p0/p) 

The barometric altitude formula as the basis for an analog masterpiece by Oris
z=Height, p=Air pressure at height z, p0=Air pressure for z=0, g=gravitational acceleration,
 RL=specific gas constant for dry air, and Tv=average virtual temperature between the air pressure levels of p0 and p
"""

# calcul de la pression reduite au niveau de la mer recommande par le service meteo allemand
def P0(pres,temp,alt):
    g0 = 9.80665  # accelaration pesanteur m/s2
    tk = float(temp)+ 263.15
    R= 287.05     # constante spécifique de l'air sec  m2/S2 K
    alpha = 0.0065 # gradient de temperature vertical  K/m
    Ch = 0.12   # coef correctif pour la pression partielle K/hPa
    E= calcE(temp)
    print ("P partielle Vap H2O:"+ str(round(E,2)))
    x0 = (g0 / (R * (tk + Ch* E + alpha*alt/2))) * float(alt)
    Pr0 =  pres * math.exp(x0)
    return Pr0

# calcul de la pression reduite au niveau de la mer
# 5.255 =  Mg/Ra  -1 soit  M=0,0289644, g=9,80665 et R=8,31446261815)
def P0_1(pres,alt):
    Pr0_1= float(pres)/(math.pow(1-((0.0065*float(alt)) / 288.15 ),5.2557859))
    return Pr0_1


def P0_2(pres,temp,alt):
    T1= 288.15 -(0.0016*float(alt))
    T2= float(temp) + 273.15   # (TMAX + TMIN) / 2 en K
    t= (T2+T1) / 2.0
    E= (9.80665 * float(alt)) / (287.08 * t)
    p0 = float(pres) * math.exp(E)
    return p0


def P0_3(pres,temp,hr,alt):
    altF = float(alt) * 3.2804
    tv= T1_tvirt(pres,temp,hr)
    tvR = (float(temp)* (9.0 / 5.0) + 32)+ 459.67 # en degre rankine
    e= altF / (122.8943111 * tvR)
    k= math.pow(10,e)
    prel= k * float(pres)
    return prel

def P0_4(pres,temp,alt):
    g = 9.80665  # m/s2
    M= 0.0289644 # kg / mol
    R= 8.31432 # Nm /molK
    tempK = temp + 273.15
    ex =math.exp((-g * M * alt)/(R * tempK))
    return pres / ex



X_SommeVect=0.0
Y_SommeVect=0.0
NbrDirValue = 0
DirectionMoy5sTemp = 0.0
DEG2GR = 0.0174533
GR2DEG = 57.2958

def FormatAngle(angle):
    lAngle = angle
  # conversion en 360°
    while (lAngle <0):
        lAngle = lAngle +360;
    while(lAngle>360):
        lAngle = lAngle - 360;
    if (lAngle==360):
             lAngle =0;
    return lAngle;


def V1_CalculDirVent(pos1,pos2):
    global SommeVectX
    global SommeVectY
    global DirectionMoy5sTemp
    global NbrDirValue

    lvX = math.sin(math.radians(pos1))
    lvY = math.cos(math.radians(pos2))

    NbrDirValue = NbrDirValue +1

    X_SommeVect = (X_SommeVect * (NbrDirValue-1) + lvX) / float(NbrDirValue);
    Y_SommeVect = (Y_SommeVect * (NbrDirValue-1) + lvY) / float(NbrDirValue);

    DirectionMoy5sTemp = math.atan2(Y_SommeVect,X_SommeVect)* GR2DEG;
    DirectionMoy5sTemp = FormatAngle(DirectionMoy5sTemp); # ramene l'angle entre 0 et 360 °

windAvgX = 0.0

def V2_CalculVent(theta):
    global windAvgX
    ltheta =math.radians(theta)
    windAvgX = windAvgX * .75 + math.cos(ltheta) * .25;
    windAvgX = windAvgX * (1 - 0.25) + math.cos(ltheta) * .25;
    windAvgX = windAvgX - windAvgX * 0.25 + math.cos(ltheta) * .25;
    windAvgX += ( math.cos(ltheta) - windAvgX) * 0.25;
    return windAvgX

def V3_CalculVent(pos1,pos2):
    moy= (pos1 + pos2) / 2
    if (math.fabs(pos1-pos2) > 180):
        moy = moy + 180
    moy = moy % 360
    return moy



def main():

    """
    TEMP = float(input('Temperaure en °C:'))
    HR = float(input('Humidite %:'))
    PRES =float(input('Pression local hPa:'))
    ALT = float(input('Altitude station m:'))
    VITV= float(input("vitesse vent km/h:"))
    """
    TEMP=10.0
    HR = 50
    PRES = 1000
    ALT = 300
    VITV= 20


    PR=CalPtRosee(TEMP, HR/100.0)
    print ('Point de rosee='+ str(round(PR,2)) + ' °C')
    HS =HumSpecif(TEMP,HR)
    print ('H spécif.='+ str(round(HS,5)) + "  Kg eau/Kg Air sec")
    tv= T1_tvirt(PRES,TEMP,HR)
    print ('T virtuelle='+ str(round(tv,2)) + " °C  " + str(round((tv * 9.0/5.0) +32.0,2)) +" °F " +  str(round(tv +459.67,2)) +" °R ")
    tv2= T2_tvirt(PRES,TEMP,HR)
    print ('T virtuelle 2 ='+ str(round(tv2,2))+" °C")


    P1_pressionMer = P0(PRES,TEMP,ALT)
    B2_BulbeHumide = T2_TempHumide(PRES,TEMP,HR)
    print ('T humid bis='+ str(round(B2_BulbeHumide,2)) +" °C")
    B3_BulbeHumide = T3_TempHumide(PRES,TEMP,HR)
    # print ('Temperature humid ter='+ str(round(BulbeHumide_ter,2)) +" °C")


    print("----------")
    print ('P niveau mer P0='+ str(round(P1_pressionMer,1)) + " hPa")
    P2_pressionMer =P0_1(PRES,ALT) # la plus precise
    print ('P niveau mer P0_1 ='+ str(round(P2_pressionMer,1)) + " hPa")
    P3_pressionMer =P0_2(PRES,TEMP,ALT)
    # print ('Pression reduite niveau mer Ter='+ str(round(pressionMerTer,1)) + " hPa")
    PRESREL =P0_3(PRES,TEMP,HR,ALT)
     #print ('Pression reduite niveau mer quad='+ str(round(PRESREL,1)) + " hPa")

    P4_pressionMer=P0_4(PRES,TEMP,ALT) # la plus precise
    print ('P niveau mer P0_4 ='+ str(round(P4_pressionMer,1)) + " hPa") # marche tres bien

    print("----------")
    PSAT =round(P1_Psat(TEMP)/ 100.0,2)
    print ('P Vap sat Psat:'+ str(PSAT) + ' hPa')
    # calcul de psatbis
    print ("P Vap sat bis Psat:"+ str(round(P2_Psat(TEMP)/100.0,2))+" hPa") # a prendre car le plus juste
    # calcul de psat ter
    print ("P Vap sat ter Psat:"+ str(round(P3_Psat(TEMP),2))+" hPa")
    # calcul de psat quad
    #print ("Pression  Vap saturante quad Psat:"+ str(round(Psat_quad(TEMP),2))+" hPa")
    # calcul de psat quint
    #print ("Pression  Vap saturante quint Psat:"+ str(round(Psat_quint(TEMP),2))+" atm")


    print("---------")

    #calcul de pvap = Psat * HR / 100
    PVAP = P2_Psat(TEMP) * HR / 100.0
    print ("P Pvap calculee:"+ str(round(PVAP/100.0,2))+" hPa")



    WindChill =wct(TEMP,VITV)
    print ('Windchill ='+ str(round(WindChill,1)) + " °C")

   # pos = position()
   # phasename = phase(pos)

   # roundedpos = round(float(pos), 3)
   # print ("%s (%s)" % (phasename, roundedpos))

    d = float(input("DATE JJ.MMAAAA"))
    h = float(input("HEURE 0-24"))
   
    pos = position(d, h)
    phase(pos)

    """

    # calcul direction du vent moyen

    CalculDirVent(359,10)
    print("---------------------------------------")
    print ("Dir moyen = " + str(DirectionMoy5sTemp))
    print("---------------------------------------")
    CalculVent_bis(359)
    print ("Dir moyen = " + str(math.degrees(windAvgX)))
    CalculVent_bis(10)

    print ("Dir moyen = " + str(math.degrees(windAvgX)))

    print ("dir moyen ter="+str(CalculVent_ter(359,10)))
    print ("dir moyen ter="+str(CalculVent_ter(170,190)))
    print ("dir moyen ter="+str(CalculVent_ter(350,2)))

    """








print("calcul meteo")
main()
