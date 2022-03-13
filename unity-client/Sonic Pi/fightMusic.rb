use_bpm 60

beatSleepTime = 14 # Controls how long the beat section lasts.
mainSleepTime = 20 # Controls how long the main section lasts.
subSleepTime = 7   # Controls how long the sub section lasts.




##
## BEAT
##

live_loop :beat1 do
  r = 0.75
  timeToRepeat = sample_duration(:ambi_choir, rate: r*2) * beatSleepTime
  
  timeToRepeat.times do
    sample :ambi_choir, rate: r #,attack: [1, 0, 0.2, 0.5].choose
    sleep sample_duration(:ambi_choir, rate: r*2)
  end
  
  sync :endSub1
end


live_loop :beat2 do
  synth :dark_ambience, note: 40, sustain: beatSleepTime, release: 1, res: 0.2
  sleep beatSleepTime
  cue :startMain1
  sync :endSub1
end


##
## MAIN
##

live_loop :main1 do
  sync :startMain1
  cue :startMain2
  
  r = 1
  mainSleepTime.times do
    sample :ambi_choir, rate: r, pitch: [2, 2, 1, 3, 3].tick, pitch_dis: 0.001
    sleep sample_duration(:ambi_choir, rate: r*2)
  end
  cue :startSub1
end


with_fx :reverb, room: 1, mix: 1 do
  live_loop :main2 do
    sync :startMain2
    r = 0.7
    (mainSleepTime*1.5).times do
      sample :ambi_choir, rate: r, pitch: 1, pitch_dis: 0.001, release: 0.01
      sleep 0.6
    end
  end
end


##
## SUB
##

live_loop :sub1 do
  sync :startSub1
  
  r = [0.2, -0.2].choose
  subSleepTime.times do
    sample :ambi_choir, rate: r, pitch: [5, 5, 12, 9, 8].tick, pitch_dis: 0.001
    sleep sample_duration(:ambi_choir, rate: r*2)
  end
  
  sleep 4.25 # wait for the sub to completely fade to zero before playing the next part of the song!
  cue :endSub1
end




