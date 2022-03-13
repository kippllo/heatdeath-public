use_bpm 120

live_loop :beat1 do
  2.times do
    synth :square, note: 40, release: 0.2 # Sounds like pokemon walk-bumb nois.
    sleep 1
  end
  synth :square, note: 80, release: 0.2 # Sounds like pokemon click noise.
  sleep 1
end


live_loop :beat2 do
  synth :square, note: 80, release: 0.2 # Sounds like pokemon click noise.
  sleep 2.5
end


##
### Main part of the song, past the beat:
##

sleep 10 # Block a bit before the main part starts.

with_fx :reverb do
  live_loop :main1 do
    3.times do
      synth :square, note: 100, release: 0.2
      sleep 5
    end
    sleep 10
  end
end

with_fx :echo do
  live_loop :main2 do
    10.times do
      synth :square, note: 100, release: 0.2
      sleep 1
    end
    with_fx :krush do
      synth :square, note: 40, sustain: 1, release: 0.2 # It needs a pop at the end!
    end
    #cue :startSub1
    #sync :overSub1 #This will make this thread wait until sub1 is done to play again!
    sleep 20
  end
end


#These start and stop at a specific time.
=begin
live_loop :sub1 do
  use_bpm 60
  sync :startSub1
  puts "sub1"
  sleep [1, 5, 15].choose
  [3, 6, 9].choose.times do
    with_fx :wobble, wave: 1 do
      rel = [8, 4, 10].choose
      synth :square, note: (ring 80, 78, 71.3).tick(), sustain: 1, release: rel, cutoff: [80, 40, 20].choose
      sleep rel
    end
  end
  cue :overSub1
end
=end


